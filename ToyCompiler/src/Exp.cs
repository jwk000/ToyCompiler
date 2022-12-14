using System;
using System.Collections.Generic;
using System.Linq;
namespace ToyCompiler;



/* 
 * exp = assign |vardecl| forin
 * assign = condition | condition assign_op assign
 * assign_op = '='|'*=' |'/='|'%='|'+='|'-='
 * vardecl = 'var' ID ('=' condition)?
 * forin = 'var' params 'in' postfix
 * condition = or | or '?' exp ':' condition
 * or = and ('||' and )*
 * and = equal ('&&' equal)*
 * equal = relation ('=='|'!=' relation)*
 * relation = add (relation_op add)*
 * relation_op = '<'|'>'|'<='|'>='
 * add = mul ('+'|'-' mul)*
 * mul = unary  ('/'|'*'|'%' unary)*
 * unary = postfix | '++' unary | '--' unary | '!+-' unary 
 * postfix = prim | postfix '++'| postfix '--' |postfix '(' args? ')'| postfix '[' exp ']'| postfix '.' ID
 * prim = ID | NUM | STR | '(' exp ')' | arr | obj
 * arr = '[' args? ']'
 * obj = '{' kvs? '}'
 * args = exp (',' exp)*
 * kvs = ID '=' exp (',' ID '=' exp)*
 * params = ID (, ID)*
 * 
 */

interface IExp
{
    bool Parse(TokenReader reader);
    Variant Calc();
}

class Exp : IExp
{
    public AssignExp assign;
    public VarDeclExp decl;
    public ForInExp forin;
    public bool Parse(TokenReader reader)
    {
        Token t = reader.Peek();
        if (t.tokenType == TokenType.TTVar)
        {
            if (reader.ContainsToken(TokenType.TTIn))
            {
                forin = new ForInExp();
                return forin.Parse(reader);
            }
            else
            {
                decl = new VarDeclExp();
                return decl.Parse(reader);
            }
        }
        else
        {
            assign = new AssignExp();
            return assign.Parse(reader);
        }
    }
    public Variant Calc()
    {
        if (decl != null)
        {
            return decl.Calc();
        }else if (forin != null)
        {
            return forin.Calc();
        }
        else
        {
            return assign.Calc();
        }
    }
}

class AssignExp : IExp
{
    public ConditionExp left;
    public Token op;
    public AssignExp right;

    public bool Parse(TokenReader reader)
    {
        left = new ConditionExp();
        if (!left.Parse(reader))
        {
            return false;
        }

        Token t = reader.Peek();
        if (t != null)
        {

            if (t.tokenType == TokenType.TTAssign
                || t.tokenType == TokenType.TTPlusAssign
                || t.tokenType == TokenType.TTMinusAssign
                || t.tokenType == TokenType.TTMultipleAssign
                || t.tokenType == TokenType.TTDivisionAssign)
            {
                reader.Next();
                op = t;
                right = new AssignExp();
                return right.Parse(reader);
            }
        }

        return true;
    }
    public Variant Calc()
    {
        if (op == null)
        {
            return left.Calc();
        }
        else if (op.tokenType == TokenType.TTAssign)
        {
            Variant v = left.Calc();
            Variant w = right.Calc();
            v.Assign(w);
            return v;
        }
        else if (op.tokenType == TokenType.TTPlusAssign)
        {
            Variant v = left.Calc();
            Variant u = v + right.Calc();
            v.Assign(u);
            return v;
        }
        else if (op.tokenType == TokenType.TTMinusAssign)
        {
            Variant v = left.Calc();
            Variant u = v - right.Calc();
            v.Assign(u);
            return v;
        }
        else if (op.tokenType == TokenType.TTMultipleAssign)
        {
            Variant v = left.Calc();
            Variant u = v * right.Calc();
            v.Assign(u);
            return v;
        }
        else if (op.tokenType == TokenType.TTDivisionAssign)
        {
            Variant v = left.Calc();
            Variant u = v / right.Calc();
            v.Assign(u);
            return v;
        }
        throw new Exception($"invalid assign op {op}");
    }

}

class VarDeclExp : IExp
{
    public Token mID;
    public ConditionExp mCond;

    public bool Parse(TokenReader reader)
    {
        Token t = reader.Next();
        if (t.tokenType != TokenType.TTVar)
        {
            return false;
        }

        t = reader.Next();
        if (t.tokenType != TokenType.TTID)
        {
            return false;
        }

        mID = t;
        t = reader.Next();
        if (t.tokenType != TokenType.TTAssign)
        {
            return false;
        }
        mCond = new ConditionExp();
        return mCond.Parse(reader);
    }

    public Variant Calc()
    {
        Variant v = new Variant();
        v.id = mID.desc;
        Variant ret = mCond.Calc();
        v.Assign(ret);
        Env.LocalScope.AddVariant(v);
        return v;
    }

}

class ForInExp : IExp
{
    public List<Token> mParams;
    public Exp mExp;
    public Variant mVar;
    public object mIterator;
    public bool Parse(TokenReader reader)
    {
        Token t = reader.Next();
        if(t.tokenType != TokenType.TTVar)
        {
            return false;
        }

        //参数列表
        var ts = reader.SeekNextToken(TokenType.TTIn);
        mParams = new List<Token>();
        TokenReader tr = new TokenReader(ts);
        t = tr.Next();
        while (t != null)
        {
            mParams.Add(t);
            t = tr.Next();
            if (t != null)
            {
                if (t.tokenType != TokenType.TTComma)
                {
                    return false;
                }
                t = tr.Next();
            }
        }

        //in后面的部分
        mExp = new Exp();
        return mExp.Parse(reader);
    }

    public Variant Calc()
    {
       var v = mExp.Calc();

        if (v.variantType == VariantType.Array)
        {
            mIterator = v.arr.GetEnumerator();
        }
        else if (v.variantType == VariantType.Object)
        {
            mIterator = v.obj.GetEnumerator();

        }
        mVar = v;
        return v;
    }

    public bool Next()
    {
        if (mVar.variantType == VariantType.Array)
        {
            IEnumerator<Variant> it = mIterator as IEnumerator<Variant>;
            if (it.MoveNext())
            {
                var t = mParams[0];
                it.Current.id = t.desc;
                Env.LocalScope.AddVariant(it.Current);
                return true;
            }
        }
        else if (mVar.variantType == VariantType.Object)
        {
            IEnumerator<KeyValuePair<string, Variant>> it = mIterator as IEnumerator<KeyValuePair<string, Variant>>;
            if(it.MoveNext())
            {
                var t = mParams[0];
                Variant v = Env.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.variantType = VariantType.String;
                    v.str = it.Current.Key;
                    v.id = t.desc;
                    Env.LocalScope.AddVariant(v);
                }
                else
                {
                    v.str = it.Current.Key;
                }

                t = mParams[1];
                Variant u = Env.LocalScope.GetVariant(t.desc);
                if (u == null)
                {
                    u = it.Current.Value;
                    u.id = t.desc;
                    Env.LocalScope.AddVariant(u);
                }
                else
                {
                    u.Assign(it.Current.Value);
                }

                return true;
            }
        }
        return false;
    }

    
}
class ConditionExp : IExp
{
    public OrExp left;
    public Exp mid;
    public ConditionExp right;

    public bool Parse(TokenReader reader)
    {
        left = new OrExp();
        if (!left.Parse(reader))
        {
            return false;
        }
        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTQuestion)
            {
                reader.Next();
                mid = new Exp();
                if (!mid.Parse(reader))
                {
                    return false;
                }
            }
            else //可以没有?
            {
                return true;
            }

            t = reader.Peek();
            if (t.tokenType == TokenType.TTColon)
            {
                reader.Next();
                right = new ConditionExp();
                if (!right.Parse(reader))
                {
                    return false;
                }
            }
            else //必须有:
            {
                return false;
            }
        }

        return true;
    }

    public Variant Calc()
    {
        var v = left.Calc();
        if (mid != null && right != null)
        {
            if (v == true)
            {
                return mid.Calc();
            }
            else
            {
                return right.Calc();
            }
        }
        return v;
    }
}

class UnaryExp : IExp
{
    public PostfixExp postfix;
    public UnaryExp unary;
    public Token op;

    public bool Parse(TokenReader reader)
    {
        Token t = reader.Peek();
        if (t.tokenType == TokenType.TTPlusPlus || t.tokenType == TokenType.TTMinusMinus || t.tokenType == TokenType.TTNot)
        {
            t = reader.Next();
            op = t;
            unary = new UnaryExp();
            return unary.Parse(reader);
        }
        postfix = new PostfixExp();
        return postfix.Parse(reader);
    }

    public Variant Calc()
    {
        if (op == null)
        {
            return postfix.Calc();
        }
        else if (op.tokenType == TokenType.TTPlusPlus)
        {
            var v = unary.Calc();
            return ++v;
        }
        else if (op.tokenType == TokenType.TTMinusMinus)
        {
            var v = unary.Calc();
            return --v;
        }
        else if (op.tokenType == TokenType.TTNot)
        {
            var v = unary.Calc();
            if (v == false) return true;
            return false;
        }
        throw new Exception($"unary operator {op} not supported");
    }
}

enum PostfixType
{
    None,
    PlusPlus,
    MinusMinus,
    FuncCall,
    Index,
    Point,
}

class PostfixExp : IExp
{
    public PrimExp prim;
    public PostfixType postfixType = PostfixType.None;
    public Token pointID;
    public List<Exp> args;
    public Exp index;
    public PostfixExp next;
    public bool first = true;

    public bool Parse(TokenReader reader)
    {
        if (first)
        {
            prim = new PrimExp();
            if (!prim.Parse(reader))
            {
                return false;
            }
        }

        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTPlusPlus) //++
            {
                reader.Next();
                postfixType = PostfixType.PlusPlus;
            }
            else if (t.tokenType == TokenType.TTMinusMinus) //--
            {
                reader.Next();
                postfixType = PostfixType.MinusMinus;
            }
            else if (t.tokenType == TokenType.TTLeftBracket) //()
            {
                postfixType = PostfixType.FuncCall;
                List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket, TokenType.TTRightBracket);
                if (ts.Count > 0)
                {
                    TokenReader tr = new TokenReader(ts);
                    args = new List<Exp>();
                    while (!tr.IsEnd())
                    {

                        var exp = new Exp();
                        if (!exp.Parse(tr))
                        {
                            return false;
                        }
                        args.Add(exp);
                        Token nt = tr.Next();
                        if (nt == null)
                        {
                            break;
                        }
                        if (nt.tokenType != TokenType.TTComma)
                        {
                            return false;
                        }

                    }

                }
            }
            else if (t.tokenType == TokenType.TTLeftBracket2)//[]
            {
                postfixType = PostfixType.Index;
                List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket2, TokenType.TTRightBracket2);
                if (ts.Count == 0)
                {
                    return false;
                }
                index = new Exp();
                index.Parse(ts);
            }
            else if (t.tokenType == TokenType.TTDot) //.
            {
                reader.Next();
                postfixType = PostfixType.Point;
                t = reader.Next();
                if (t.tokenType != TokenType.TTID)
                {
                    return false;
                }
                pointID = t;
            }
            else //遇到不识别的后缀就结束判定，交给上层处理
            {
                return true;
            }

            //多级后缀表达式
            t = reader.Peek();
            if (t != null)
            {
                next = new PostfixExp();
                next.first = false;
                return next.Parse(reader);
            }

        }

        return true;
    }

    public Variant Calc()
    {
        var v = prim.Calc();
        if (postfixType == PostfixType.None)
        {
            return v;
        }
        v = _calc(this, v);
        var _next = next;
        while (_next != null)
        {
            v = _calc(_next, v);
            _next = next.next;
        }

        return v;
    }

    static Variant _calc(PostfixExp p, Variant v)
    {
        if (p.postfixType == PostfixType.PlusPlus)
        {

            return v++;
        }
        else if (p.postfixType == PostfixType.MinusMinus)
        {

            return v--;
        }
        else if (p.postfixType == PostfixType.FuncCall)
        {
            var f = Env.LocalScope.GetVariant(v.id);
            if (f != null)
            {
                var stat = f.fun;
                //参数压栈
                foreach (var exp in p.args)
                {
                    Env.RunList.Add(exp.Calc());
                }
                stat.Call(Env.LocalScope);
                if (Env.RunList.Count > 0)
                {
                    v = Env.RunList[0];
                    Env.RunList.Clear();
                    return v;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new Exception($"invalid function call {v.id}");
            }
        }
        else if (p.postfixType == PostfixType.Index)//array
        {
            Variant va = Env.LocalScope.GetVariant(v.id);
            if (va == null || va.arr == null)
            {
                throw new Exception($"invalid array index {v.id}");
            }
            VArray arr = va.arr;
            return arr.GetAt((int)(double)p.index.Calc());
        }
        else if (p.postfixType == PostfixType.Point)//object
        {
            Variant va = Env.LocalScope.GetVariant(v.id);
            if (va == null || va.obj == null)
            {
                throw new Exception($"invalid object access {v.id}");
            }
            VObject obj = va.obj;
            return obj.Get(p.pointID.desc);
        }
        else
        {
            throw new Exception($"invalid postifx type {p.postfixType}");
        }
    }
}


class OrExp : IExp
{
    public AndExp left;
    public OrExp right;

    public bool Parse(TokenReader reader)
    {

        left = new AndExp();
        if (!left.Parse(reader))
        {
            return false;
        }

        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTOr)
            {
                reader.Next();
                right = new OrExp();
                return right.Parse(reader);
            }
        }

        return true;
    }

    public Variant Calc()
    {
        if (right == null)
        {
            return left.Calc();
        }
        else
        {
            return (bool)left.Calc() || (bool)right.Calc();
        }
    }
}

class AndExp : IExp
{
    public EqualExp left;
    public AndExp right;

    public bool Parse(TokenReader reader)
    {
        left = new EqualExp();
        if (!left.Parse(reader))
        {
            return false;
        }

        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTAnd)
            {
                reader.Next();
                right = new AndExp();
                return right.Parse(reader);
            }
        }


        return true;

    }

    public Variant Calc()
    {
        if (right == null)
        {
            return left.Calc();
        }
        else
        {
            return (bool)left.Calc() && (bool)right.Calc();
        }
    }
}

class EqualExp : IExp
{
    public RelationExp left;
    public EqualExp right;
    public Token op;

    public bool Parse(TokenReader reader)
    {
        left = new RelationExp();
        if (!left.Parse(reader))
        {
            return false;
        }


        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTEqual || t.tokenType == TokenType.TTNotEqual)
            {
                reader.Next();
                op = t;
                right = new EqualExp();
                return right.Parse(reader);
            }
        }

        return true;

    }

    public Variant Calc()
    {
        if (op == null)
        {
            return left.Calc();
        }
        else if (op.tokenType == TokenType.TTEqual)
        {
            return left.Calc() == right.Calc();
        }
        else if (op.tokenType == TokenType.TTNotEqual)
        {
            return left.Calc() != right.Calc();
        }
        throw new Exception($"equal not support {op}");
    }
}

class RelationExp : IExp
{
    public PlusExp left;
    public RelationExp right;
    public Token op;

    public bool Parse(TokenReader reader)
    {
        left = new PlusExp();
        if (!left.Parse(reader))
        {
            return false;
        }


        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTLessThan || t.tokenType == TokenType.TTLessEqual || t.tokenType == TokenType.TTGreatEqual || t.tokenType == TokenType.TTGreatThan)
            {
                reader.Next();
                op = t;
                right = new RelationExp();
                return right.Parse(reader);
            }
        }

        return true;

    }

    public Variant Calc()
    {
        if (op == null)
        {
            return left.Calc();
        }
        else if (op.tokenType == TokenType.TTLessThan)
        {
            return left.Calc() < right.Calc();
        }
        else if (op.tokenType == TokenType.TTLessEqual)
        {
            return left.Calc() <= right.Calc();
        }
        else if (op.tokenType == TokenType.TTGreatThan)
        {
            return left.Calc() > right.Calc();
        }
        else if (op.tokenType == TokenType.TTGreatEqual)
        {
            return left.Calc() >= right.Calc();
        }
        throw new Exception($"relation not suport {op}");
    }
}

class PlusExp : IExp
{
    public MulExp left;
    public PlusExp right;
    public Token op;

    public bool Parse(TokenReader reader)
    {
        left = new MulExp();
        if (!left.Parse(reader))
        {
            return false;
        }
        Token t = reader.Peek();

        if (t != null)
        {
            if (t.tokenType == TokenType.TTPlus || t.tokenType == TokenType.TTMinus)
            {
                reader.Next();
                op = t;
                right = new PlusExp();
                if (!right.Parse(reader))
                {
                    return false;
                }
            }

        }

        return true;
    }

    public Variant Calc()
    {
        if (op == null)
        {
            return left.Calc();
        }
        else if (op.tokenType == TokenType.TTPlus)
        {
            return left.Calc() + right.Calc();
        }
        else
        {
            return left.Calc() - right.Calc();
        }
    }
}

class MulExp : IExp
{
    public UnaryExp left;
    public MulExp right;
    public Token op;

    public bool Parse(TokenReader reader)
    {

        left = new UnaryExp();
        if (!left.Parse(reader))
        {
            return false;
        }
        Token t = reader.Peek();
        if (t != null)
        {
            if (t.tokenType == TokenType.TTMultiple || t.tokenType == TokenType.TTDivision)
            {
                reader.Next();
                op = t;
                right = new MulExp();
                if (!right.Parse(reader))
                {
                    return false;
                }
            }

        }

        return true;
    }
    public Variant Calc()
    {
        if (op == null)
        {
            return left.Calc();
        }
        if (op.tokenType == TokenType.TTMultiple)
        {
            return left.Calc() * right.Calc();
        }
        else
        {
            return left.Calc() / right.Calc();
        }
    }
}

enum PrimExpType
{
    Invalid,
    Number,
    String,
    ID,
    Boolean,
    Exp,
    Array,
    Object
}
class PrimExp : IExp
{
    public PrimExpType primType = PrimExpType.Invalid;
    public Token token;
    public Exp exp;
    public ArrayExp arr;
    public ObjectExp obj;

    public bool Parse(TokenReader reader)
    {
        Token t = reader.Peek();

        if (t.tokenType == TokenType.TTNum)
        {
            reader.Next();
            primType = PrimExpType.Number;
            token = t;
            return true;
        }
        else if (t.tokenType == TokenType.TTString)
        {
            reader.Next();
            primType = PrimExpType.String;
            token = t;
            return true;
        }
        else if (t.tokenType == TokenType.TTID)
        {
            reader.Next();
            primType = PrimExpType.ID;
            token = t;
            return true;
        }
        else if (t.tokenType == TokenType.TTBool)
        {
            reader.Next();
            primType = PrimExpType.Boolean;
            token = t;
            return true;
        }
        else if (t.tokenType == TokenType.TTLeftBracket)
        {
            primType = PrimExpType.Exp;
            List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket, TokenType.TTRightBracket);
            exp = new Exp();
            return exp.Parse(ts);
        }
        else if (t.tokenType == TokenType.TTLeftBracket2)
        {
            primType = PrimExpType.Array;
            arr = new ArrayExp();
            return arr.Parse(reader);
        }
        else if (t.tokenType == TokenType.TTLeftBracket3)
        {
            primType = PrimExpType.Object;
            obj = new ObjectExp();
            return obj.Parse(reader);
        }
        else
        {
            Console.WriteLine("invalid prim token {0}", t);
            return false;
        }


    }
    public Variant Calc()
    {
        if (primType == PrimExpType.Number)
        {
            return double.Parse(token.desc);
        }
        else if (primType == PrimExpType.String)
        {
            return token.desc;
        }
        else if (primType == PrimExpType.Boolean)
        {
            return bool.Parse(token.desc);
        }
        else if (primType == PrimExpType.ID)
        {
            var v = Env.LocalScope.GetVariant(token.desc);
            if (v != null)
            {
                return v;
            }

            throw new Exception($"find not decleared variant id {token}");
        }
        else if (primType == PrimExpType.Exp)
        {

            return exp.Calc();
        }
        else if (primType == PrimExpType.Array)
        {
            return arr.Calc();
        }
        else if (primType == PrimExpType.Object)
        {
            return obj.Calc();
        }
        else
        {
            throw new Exception("invalid prim type!");
        }
    }
}

class ArrayExp : IExp
{
    public List<Exp> args;

    public bool Parse(TokenReader reader)
    {
        Token t = reader.Peek();
        if (t.tokenType != TokenType.TTLeftBracket2)
        {
            return false;
        }
        List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket2, TokenType.TTRightBracket2);
        if (ts.Count > 0)
        {
            TokenReader tr = new TokenReader(ts);
            args = new List<Exp>();
            while (!tr.IsEnd())
            {
                var exp = new Exp();
                if (!exp.Parse(tr))
                {
                    return false;
                }
                args.Add(exp);
                Token nt = tr.Next();
                if (nt == null)
                {
                    break;
                }
                if (nt.tokenType != TokenType.TTComma)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public Variant Calc()
    {
        Variant v = new Variant();
        v.variantType = VariantType.Array;
        v.arr = new VArray();
        if (args != null)
        {

            foreach (var exp in args)
            {
                v.arr.Add(exp.Calc());
            }
        }
        return v;
    }
}

class ObjectExp : IExp
{
    public Dictionary<string, Exp> kvs;
    public bool Parse(TokenReader reader)
    {
        Token t = reader.Peek();
        if (t.tokenType != TokenType.TTLeftBracket3)
        {
            return false;
        }
        List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket3, TokenType.TTRightBracket3);
        if (ts.Count > 0)
        {
            TokenReader tr = new TokenReader(ts);
            kvs = new Dictionary<string, Exp>();
            while (!tr.IsEnd())
            {
                t = tr.Next();
                if (t.tokenType != TokenType.TTID)
                {
                    return false;
                }
                string k = t.desc;

                t = tr.Next();
                if(t.tokenType != TokenType.TTAssign)
                {
                    return false;
                }

                var exp = new Exp();
                if (!exp.Parse(tr))
                {
                    return false;
                }
                kvs.Add(k, exp);

                t = tr.Next();
                if (t == null)
                {
                    break;
                }
                if (t.tokenType != TokenType.TTComma)
                {
                    return false;
                }
            }
        }

        return true;

    }

    public Variant Calc()
    {
        Variant v = new Variant();
        v.variantType = VariantType.Object;
        v.obj = new VObject();
        foreach (var kv in kvs)
        {
            v.obj.Add(kv.Key, kv.Value.Calc());
        }
        return v;
    }
}

