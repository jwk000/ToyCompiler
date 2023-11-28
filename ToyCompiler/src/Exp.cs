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
    void OnVisit(List<Instruction> code);
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
                decl = new VarDeclExp(); //变量声明
                return decl.Parse(reader);
            }
        }
        else
        {
            assign = new AssignExp();//变量赋值
            return assign.Parse(reader);
        }
    }
    public Variant Calc()
    {
        if (decl != null)
        {
            return decl.Calc();
        }
        else if (forin != null)
        {
            return forin.Calc();
        }
        else
        {
            return assign.Calc();
        }
    }
    public void OnVisit(List<Instruction> code)
    {
        if (decl != null)
        {
            decl.OnVisit(code);
        }
        else if(forin != null)
        {
            forin.OnVisit(code);
        }
        else
        {
            assign.OnVisit(code);
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

    public void OnVisit(List<Instruction> code)
    {
        if (op == null)
        {
            left.OnVisit(code);
        }
        else if (op.tokenType == TokenType.TTAssign)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTPlusAssign)
        {
            left.OnVisit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt=1});
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Add));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTMinusAssign)
        {
            left.OnVisit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Sub));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTMultipleAssign)
        {
            left.OnVisit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Mul));
            code.Add(new Instruction(OpCode.Assign));

        }
        else if (op.tokenType == TokenType.TTDivisionAssign)
        {
            left.OnVisit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Div));
            code.Add(new Instruction(OpCode.Assign));
        }

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
        Env.LocalScope.SetVariant(v);
        return v;
    }

    public void OnVisit(List<Instruction> code)
    {
        mCond.OnVisit(code);
        code.Add(new Instruction(OpCode.Store) { OpStr = mID.desc });
    }

}
//for(var item in arr)
class ForInExp : IExp
{
    public List<Token> mParams;//in前面的参数
    public Exp mExp;//in后面的表达式
    public Variant mVar;//exp的计算结果
    public object mIterator;//var的迭代器
    public int mIndex = 0;//数组下标
    public bool Parse(TokenReader reader)
    {
        Token t = reader.Next();
        if (t.tokenType != TokenType.TTVar)
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

    public void OnVisit(List<Instruction> code)
    {
        mExp.OnVisit(code);
        code.Add(new Instruction(OpCode.Enum));
    }

    public bool Next()
    {
        if (mVar.variantType == VariantType.Array)
        {
            IEnumerator<Variant> it = mIterator as IEnumerator<Variant>;
            if (it.MoveNext())
            {
                //下标
                var t = mParams[0];
                Variant v = Env.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.variantType = VariantType.Number;
                    v.id = t.desc;
                    Env.LocalScope.SetVariant(v);
                }
                v.num = mIndex++;

                //值
                t = mParams[1];
                v = Env.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.id = t.desc;
                    Env.LocalScope.SetVariant(v);
                }
                v.Assign(it.Current);

                return true;
            }
            else
            {
                mIndex = 0;
            }
        }
        else if (mVar.variantType == VariantType.Object)
        {
            IEnumerator<KeyValuePair<string, Variant>> it = mIterator as IEnumerator<KeyValuePair<string, Variant>>;
            if (it.MoveNext())
            {
                //key
                var t = mParams[0];
                Variant v = Env.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.variantType = VariantType.String;
                    v.id = t.desc;
                    Env.LocalScope.SetVariant(v);
                }
                v.str = it.Current.Key;

                //值
                t = mParams[1];
                Variant u = Env.LocalScope.GetVariant(t.desc);
                if (u == null)
                {
                    u = new Variant();
                    u.id = t.desc;
                    Env.LocalScope.SetVariant(u);
                }
                u.Assign(it.Current.Value);

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

    public void OnVisit(List<Instruction> code)
    {
        left.OnVisit(code);
        if (mid != null && right != null)
        {
            Instruction njump = new Instruction(OpCode.JumpFalse);
            code.Add(njump);
            mid.OnVisit(code);
            Instruction jumpEnd = new Instruction(OpCode.Jump);
            code.Add(jumpEnd);
            njump.OpInt = code.Count;
            right.OnVisit(code);
            jumpEnd.OpInt = code.Count;
        }
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

    public void OnVisit(List<Instruction> code)
    {
        if (op == null)
        {
            postfix.OnVisit(code);
        }
        else if (op.tokenType == TokenType.TTPlusPlus)
        {
            unary.OnVisit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            code.Add(new Instruction(OpCode.Push) { OpVar = 1 });
            code.Add(new Instruction(OpCode.Add));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTMinusMinus)
        {
            unary.OnVisit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            code.Add(new Instruction(OpCode.Push) { OpVar = 1 });
            code.Add(new Instruction(OpCode.Sub));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTNot)
        {
            unary.OnVisit(code);
            code.Add(new Instruction(OpCode.Not));
        }
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
    public PrimExp prim;//当前exp
    public PostfixType postfixType = PostfixType.None;
    public Token pointID;//obj.后面的东西
    public List<Exp> args;//func(里面的东西)
    public Exp index;//arr[下标]
    public PostfixExp next;//多级后缀
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
                var _next = new PostfixExp();
                _next.first = false;
                _next.Parse(reader);
                if (_next.postfixType != PostfixType.None)
                {
                    next = _next;
                }
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
            _next = _next.next;
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
                    Env.RunStack.Add(exp.Calc());
                }
                stat.Call(Env.LocalScope);
                if (Env.RunStack.Count > 0)
                {
                    v = Env.RunStack[0];
                    Env.RunStack.Clear();
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

    public void OnVisit(List<Instruction> code)
    {
        prim.OnVisit(code);
        if (postfixType == PostfixType.None)
        {
            return;
        }
        _OnVisit(code);
        var _next = next;
        while (_next != null)
        {
            _next.OnVisit(code);
            _next = _next.next;
        }

    }

    void _OnVisit(List<Instruction> code)
    {
        if (postfixType == PostfixType.PlusPlus)
        {
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            code.Add(new Instruction(OpCode.Push) { OpVar = 1 });
            code.Add(new Instruction(OpCode.Add));
            code.Add(new Instruction(OpCode.Assign));

        }
        else if (postfixType == PostfixType.MinusMinus)
        {
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            code.Add(new Instruction(OpCode.Push) { OpVar = 1 });
            code.Add(new Instruction(OpCode.Sub));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (postfixType == PostfixType.FuncCall)
        {
            //栈：函数变量，参数列表，参数数量，返回地址
            //参数压栈
            foreach (var exp in args)
            {
                exp.OnVisit(code);
            }
            //参数数量
            code.Add(new Instruction(OpCode.Push) { OpVar = args.Count });
            //返回地址
            code.Add(new Instruction(OpCode.Push) { OpVar = code.Count +2});
            //调用就是跳转
            code.Add(new Instruction(OpCode.Call));
        }

        else if (postfixType == PostfixType.Index)//array
        {
            code.Add(new Instruction(OpCode.Index));
        }
        else if (postfixType == PostfixType.Point)//object
        {
            code.Add(new Instruction(OpCode.Dot));
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

    public void OnVisit(List<Instruction> code)
    {
        if (right == null)
        {
            left.OnVisit(code);
        }
        else
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Or));
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

    public void OnVisit(List<Instruction> code)
    {
        if (right == null)
        {
            left.OnVisit(code);
        }
        else
        {
            left.OnVisit(code);
            right.OnVisit(code);
            Instruction ins = new Instruction(OpCode.And);
            code.Add(ins);
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

    public void OnVisit(List<Instruction> code)
    {
        if (op == null)
        {
            left.OnVisit(code);
        }
        else if (op.tokenType == TokenType.TTEqual)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.EQ));

        }
        else if (op.tokenType == TokenType.TTNotEqual)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.NE));
        }
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

    public void OnVisit(List<Instruction> code)
    {
        if (op == null)
        {
            left.OnVisit(code);
        }
        else if (op.tokenType == TokenType.TTLessThan)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.LT));
        }
        else if (op.tokenType == TokenType.TTLessEqual)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.LE));
        }
        else if (op.tokenType == TokenType.TTGreatThan)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.GT));
        }
        else if (op.tokenType == TokenType.TTGreatEqual)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.GE));
        }
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

    public void OnVisit(List<Instruction> code)
    {
        if (op == null)
        {
            left.OnVisit(code);
        }
        else if (op.tokenType == TokenType.TTPlus)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Add));
        }
        else
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Sub));
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
        else if (op.tokenType == TokenType.TTMultiple)
        {
            return left.Calc() * right.Calc();
        }
        else
        {
            return left.Calc() / right.Calc();
        }
    }

    public void OnVisit(List<Instruction> code)
    {
        if (op == null)
        {
            left.OnVisit(code);
        }
        else if (op.tokenType == TokenType.TTMultiple)
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Mul));
        }
        else
        {
            left.OnVisit(code);
            right.OnVisit(code);
            code.Add(new Instruction(OpCode.Div));
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

    public void OnVisit(List<Instruction> code)
    {
        if (primType == PrimExpType.Number)
        {
            double d = double.Parse(token.desc);
            code.Add(new Instruction(OpCode.Push) { OpVar = d });
        }
        else if (primType == PrimExpType.String)
        {
            code.Add(new Instruction(OpCode.Push) { OpVar = token.desc });
        }
        else if (primType == PrimExpType.Boolean)
        {
            bool b = bool.Parse(token.desc);
            code.Add(new Instruction(OpCode.Push) { OpVar = b });
        }
        else if (primType == PrimExpType.ID)
        {
            code.Add(new Instruction(OpCode.Load) { OpStr = token.desc });
        }
        else if (primType == PrimExpType.Exp)
        {
            exp.OnVisit(code);
        }
        else if (primType == PrimExpType.Array)
        {
            arr.OnVisit(code);
        }
        else if (primType == PrimExpType.Object)
        {
            obj.OnVisit(code);
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

    public void OnVisit(List<Instruction> code)
    {
        Variant v = new Variant();
        v.variantType = VariantType.Array;

        if (args != null)
        {
            foreach (var exp in args)
            {
                exp.OnVisit(code);
            }
        }
        code.Add(new Instruction(OpCode.Push) { OpVar = v });
        code.Add(new Instruction(OpCode.NewArray) { OpInt = args.Count });
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
                if (t.tokenType != TokenType.TTAssign)
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

    public void OnVisit(List<Instruction> code)
    {
        Variant v = new Variant();
        v.variantType = VariantType.Object;
        foreach (var kv in kvs)
        {
            code.Add(new Instruction(OpCode.Push) { OpVar = kv.Key });
            kv.Value.OnVisit(code);
        }
        code.Add(new Instruction(OpCode.Push) { OpVar = v });
        code.Add(new Instruction(OpCode.NewObj) { OpInt=kvs.Count});
    }
}

