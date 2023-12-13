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
 * prim = ID | NUM | STR | '(' exp ')' | arr | obj | co
 * arr = '[' args? ']'
 * obj = '{' kvs? '}'
 * co = 'coyield'|'coresume'
 * args = exp (',' exp)*
 * kvs = ID '=' exp (',' ID '=' exp)*
 * params = ID (, ID)*
 * 
 */

enum ExpType
{
    ETNone,
    ETAssign,
    ETVarDecl,
    ETForIn,
    ETCondition,
    ETOr,
    ETAnd,
    ETNot,
    ETEqual,
    ETRelation,
    ETAdd,
    ETMul,
    ETPreInc,
    ETPosInc,
    ETCall,
    ETPrim,

}

interface IExp
{
    bool Parse(TokenReader reader);
    Variant Calc();
    void Visit(List<Instruction> code);
    ExpType GetExpType();
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
    public ExpType GetExpType()
    {
        if (assign != null) return assign.GetExpType();
        if (decl != null) return decl.GetExpType();
        if (forin != null) return forin.GetExpType();
        return ExpType.ETNone;
    }

    public void MaybeAddClear(List<Instruction> code)
    {
        switch (GetExpType())
        {
            case ExpType.ETAssign:
            case ExpType.ETPreInc:
            case ExpType.ETPosInc:
            case ExpType.ETCall:
                code.Add(new Instruction(OpCode.Clear));
                break;
            default:
                break;
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
    public void Visit(List<Instruction> code)
    {
        if (decl != null)
        {
            decl.Visit(code);
        }
        else if (forin != null)
        {
            forin.Visit(code);
        }
        else
        {
            assign.Visit(code);
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

    public ExpType GetExpType()
    {
        if (op == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETAssign;
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

    public void Visit(List<Instruction> code)
    {
        if (op == null)
        {
            left.Visit(code);
        }
        else if (op.tokenType == TokenType.TTAssign)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTPlusAssign)
        {
            left.Visit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.Visit(code);
            code.Add(new Instruction(OpCode.Add));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTMinusAssign)
        {
            left.Visit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.Visit(code);
            code.Add(new Instruction(OpCode.Sub));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTMultipleAssign)
        {
            left.Visit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.Visit(code);
            code.Add(new Instruction(OpCode.Mul));
            code.Add(new Instruction(OpCode.Assign));

        }
        else if (op.tokenType == TokenType.TTDivisionAssign)
        {
            left.Visit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            right.Visit(code);
            code.Add(new Instruction(OpCode.Div));
            code.Add(new Instruction(OpCode.Assign));
        }

    }
}

//多值声明
class VarDeclExp : IExp
{
    public List<Token> mIds;
    public List<ConditionExp> mConds;

    public bool Parse(TokenReader reader)
    {
        Token t = reader.Next();
        if (t.tokenType != TokenType.TTVar)
        {
            return false;
        }
        mIds = new List<Token>();
        while (reader.Peek().tokenType != TokenType.TTAssign)
        {
            t = reader.Next();
            if (t.tokenType != TokenType.TTID)
            {
                return false;
            }
            mIds.Add(t);

            if (reader.Peek().tokenType == TokenType.TTAssign)
            {
                break;
            }

            t = reader.Next();
            if (t.tokenType != TokenType.TTComma)
            {
                return false;
            }
        }


        t = reader.Next();
        if (t.tokenType != TokenType.TTAssign)
        {
            return false;
        }

        List<Token> ts = reader.SeekNextToken(TokenType.TTSimicolon);
        if (ts.Count <= 0)
        {
            return false;
        }
        TokenReader tr = new TokenReader(ts);
        mConds = new List<ConditionExp>();
        while (!tr.IsEnd())
        {
            var exp = new ConditionExp();
            if (!exp.Parse(tr))
            {
                return false;
            }
            mConds.Add(exp);
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
        if (mIds.Count != mConds.Count)
        {
            return false;
        }
        return true;
    }
    public ExpType GetExpType()
    {
        return ExpType.ETVarDecl;
    }
    public Variant Calc()
    {
        for (int i = 0; i < mIds.Count; i++)
        {
            Variant v = new Variant();
            v.id = mIds[i].desc;
            Variant ret = mConds[i].Calc();
            v.Assign(ret);
            Executor.LocalScope.SetVariant(v);
        }
        return null;
    }

    public void Visit(List<Instruction> code)
    {
        for (int i = 0; i < mIds.Count; i++)
        {
            mConds[i].Visit(code);
            code.Add(new Instruction(OpCode.Store) { OpStr = mIds[i].desc });
        }
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

    public ExpType GetExpType()
    {
        return ExpType.ETForIn;
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

    public void Visit(List<Instruction> code)
    {
        mExp.Visit(code);
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
                Variant v = Executor.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.variantType = VariantType.Number;
                    v.id = t.desc;
                    Executor.LocalScope.SetVariant(v);
                }
                v.num = mIndex++;

                //值
                t = mParams[1];
                v = Executor.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.id = t.desc;
                    Executor.LocalScope.SetVariant(v);
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
                Variant v = Executor.LocalScope.GetVariant(t.desc);
                if (v == null)
                {
                    v = new Variant();
                    v.variantType = VariantType.String;
                    v.id = t.desc;
                    Executor.LocalScope.SetVariant(v);
                }
                v.str = it.Current.Key;

                //值
                t = mParams[1];
                Variant u = Executor.LocalScope.GetVariant(t.desc);
                if (u == null)
                {
                    u = new Variant();
                    u.id = t.desc;
                    Executor.LocalScope.SetVariant(u);
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

    public ExpType GetExpType()
    {
        if (mid == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETCondition;
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

    public void Visit(List<Instruction> code)
    {
        left.Visit(code);
        if (mid != null && right != null)
        {
            Instruction njump = new Instruction(OpCode.NJump);
            code.Add(njump);
            mid.Visit(code);
            Instruction jumpEnd = new Instruction(OpCode.Jump);
            code.Add(jumpEnd);
            njump.OpInt = code.Count;
            right.Visit(code);
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

    public ExpType GetExpType()
    {
        if (op == null)
        {
            return postfix.GetExpType();
        }
        if (op.tokenType == TokenType.TTPlusPlus || op.tokenType == TokenType.TTMinusMinus)
        {
            return ExpType.ETPreInc;
        }
        return ExpType.ETNot;
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

    public void Visit(List<Instruction> code)
    {
        if (op == null)
        {
            postfix.Visit(code);
        }
        else if (op.tokenType == TokenType.TTPlusPlus)
        {
            unary.Visit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            code.Add(new Instruction(OpCode.Push) { OpVar = 1 });
            code.Add(new Instruction(OpCode.Add));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTMinusMinus)
        {
            unary.Visit(code);
            code.Add(new Instruction(OpCode.SLoad) { OpInt = 1 });
            code.Add(new Instruction(OpCode.Push) { OpVar = 1 });
            code.Add(new Instruction(OpCode.Sub));
            code.Add(new Instruction(OpCode.Assign));
        }
        else if (op.tokenType == TokenType.TTNot)
        {
            unary.Visit(code);
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
    public Token dotToken;//obj.后面的东西
    public List<Exp> funArgs;//func(里面的东西)
    public Exp indexExp;//arr[下标]
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
            else if (t.tokenType == TokenType.TTLeftBracket1) //()
            {
                postfixType = PostfixType.FuncCall;
                funArgs = new List<Exp>();
                List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket1, TokenType.TTRightBracket1);
                if (ts.Count > 0)
                {
                    TokenReader tr = new TokenReader(ts);
                    while (!tr.IsEnd())
                    {

                        var exp = new Exp();
                        if (!exp.Parse(tr))
                        {
                            return false;
                        }
                        funArgs.Add(exp);
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
                indexExp = new Exp();
                indexExp.Parse(ts);
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
                dotToken = t;
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

    public ExpType GetExpType()
    {
        if (postfixType == PostfixType.None)
        {
            return prim.GetExpType();
        }
        if (postfixType == PostfixType.FuncCall)
        {
            return ExpType.ETCall;
        }
        if (postfixType == PostfixType.PlusPlus || postfixType == PostfixType.MinusMinus)
        {
            return ExpType.ETPosInc;
        }
        return ExpType.ETPrim;
    }
    public Variant Calc()
    {
        var v = prim.Calc();
        if (postfixType == PostfixType.None)
        {
            return v;
        }
        v = OnCalc(this, v);
        var _next = next;
        while (_next != null)
        {
            v = OnCalc(_next, v);
            _next = _next.next;
        }

        return v;
    }

    static Variant OnCalc(PostfixExp p, Variant v)
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
            var f = Executor.LocalScope.GetVariant(v.id);
            if (f != null)
            {
                var stat = f.fun;
                //参数压栈
                foreach (var exp in p.funArgs)
                {
                    Executor.RunStack.Add(exp.Calc());
                }
                stat.Call(Executor.LocalScope);
                if (Executor.RunStack.Count > 0)
                {
                    v = Executor.RunStack[0];
                    Executor.RunStack.Clear();
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
            Variant va = Executor.LocalScope.GetVariant(v.id);
            if (va == null || va.arr == null)
            {
                throw new Exception($"invalid array index {v.id}");
            }
            VArray arr = va.arr;
            return arr.GetAt((int)(double)p.indexExp.Calc());
        }
        else if (p.postfixType == PostfixType.Point)//object
        {
            Variant va = Executor.LocalScope.GetVariant(v.id);
            if (va == null || va.obj == null)
            {
                throw new Exception($"invalid object access {v.id}");
            }
            VObject obj = va.obj;
            return obj.Get(p.dotToken.desc);
        }
        else
        {
            throw new Exception($"invalid postifx type {p.postfixType}");
        }
    }

    public void Visit(List<Instruction> code)
    {
        if (first) prim.Visit(code);

        if (prim.primType == PrimExpType.CoYield)
        {
            //参数压栈
            foreach (var exp in funArgs)
            {
                exp.Visit(code);
            }
            //参数数量
            code.Add(new Instruction(OpCode.Push) { OpVar = funArgs.Count });

            code.Add(new Instruction(OpCode.CoYield));
        }
        else if (prim.primType == PrimExpType.CoResume)
        {
            //参数压栈
            foreach (var exp in funArgs)
            {
                exp.Visit(code);
            }
            //参数数量
            code.Add(new Instruction(OpCode.Push) { OpVar = funArgs.Count-1 });

            code.Add(new Instruction(OpCode.CoResume));
        }
        else
        {
            OnVisit(code);
            var _next = next;
            while (_next != null)
            {
                _next.Visit(code);
                _next = _next.next;
            }
        }
    }

    void OnVisit(List<Instruction> code)
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
            foreach (var exp in funArgs)
            {
                exp.Visit(code);
            }
            //参数数量
            code.Add(new Instruction(OpCode.Push) { OpVar = funArgs.Count });
            //返回地址
            code.Add(new Instruction(OpCode.Push) { OpVar = code.Count + 2 });
            //调用就是跳转
            code.Add(new Instruction(OpCode.Call));
        }

        else if (postfixType == PostfixType.Index)//array
        {
            indexExp.Visit(code);
            code.Add(new Instruction(OpCode.Index));
        }
        else if (postfixType == PostfixType.Point)//object
        {
            code.Add(new Instruction(OpCode.Dot) { OpStr = dotToken.desc });
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

    public ExpType GetExpType()
    {
        if (right == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETOr;
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

    public void Visit(List<Instruction> code)
    {
        if (right == null)
        {
            left.Visit(code);
        }
        else
        {
            left.Visit(code);
            right.Visit(code);
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

    public ExpType GetExpType()
    {
        if (right == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETAnd;
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

    public void Visit(List<Instruction> code)
    {
        if (right == null)
        {
            left.Visit(code);
        }
        else
        {
            left.Visit(code);
            right.Visit(code);
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

    public ExpType GetExpType()
    {
        if (op == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETEqual;
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

    public void Visit(List<Instruction> code)
    {
        if (op == null)
        {
            left.Visit(code);
        }
        else if (op.tokenType == TokenType.TTEqual)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.EQ));
        }
        else if (op.tokenType == TokenType.TTNotEqual)
        {
            left.Visit(code);
            right.Visit(code);
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

    public ExpType GetExpType()
    {
        if (op == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETRelation;
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

    public void Visit(List<Instruction> code)
    {
        if (op == null)
        {
            left.Visit(code);
        }
        else if (op.tokenType == TokenType.TTLessThan)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.LT));
        }
        else if (op.tokenType == TokenType.TTLessEqual)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.LE));
        }
        else if (op.tokenType == TokenType.TTGreatThan)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.GT));
        }
        else if (op.tokenType == TokenType.TTGreatEqual)
        {
            left.Visit(code);
            right.Visit(code);
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

    public ExpType GetExpType()
    {
        if (op == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETAdd;
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

    public void Visit(List<Instruction> code)
    {
        if (op == null)
        {
            left.Visit(code);
        }
        else if (op.tokenType == TokenType.TTPlus)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.Add));
        }
        else
        {
            left.Visit(code);
            right.Visit(code);
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

    public ExpType GetExpType()
    {
        if (op == null)
        {
            return left.GetExpType();
        }
        return ExpType.ETMul;
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

    public void Visit(List<Instruction> code)
    {
        if (op == null)
        {
            left.Visit(code);
        }
        else if (op.tokenType == TokenType.TTMultiple)
        {
            left.Visit(code);
            right.Visit(code);
            code.Add(new Instruction(OpCode.Mul));
        }
        else
        {
            left.Visit(code);
            right.Visit(code);
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
    Object,
    CoYield,
    CoResume
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
        else if (t.tokenType == TokenType.TTCoYield)
        {
            reader.Next();
            primType = PrimExpType.CoYield;
            token = t;
            return true;
        }
        else if (t.tokenType == TokenType.TTCoResume)
        {
            reader.Next();
            primType = PrimExpType.CoResume;
            token = t;
            return true;
        }
        else if (t.tokenType == TokenType.TTLeftBracket1)
        {
            primType = PrimExpType.Exp;
            List<Token> ts = reader.SeekMatchBracket(TokenType.TTLeftBracket1, TokenType.TTRightBracket1);
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

    public ExpType GetExpType()
    {
        if (primType == PrimExpType.Exp)
        {
            return exp.GetExpType();
        }

        return ExpType.ETPrim;
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
            var v = Executor.LocalScope.GetVariant(token.desc);
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

    public void Visit(List<Instruction> code)
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
            exp.Visit(code);
        }
        else if (primType == PrimExpType.Array)
        {
            arr.Visit(code);
        }
        else if (primType == PrimExpType.Object)
        {
            obj.Visit(code);
        }

        else
        {
            //throw new Exception("invalid prim type!");
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

    public ExpType GetExpType()
    {
        return ExpType.ETPrim;
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

    public void Visit(List<Instruction> code)
    {
        if (args != null)
        {
            foreach (var exp in args)
            {
                exp.Visit(code);
            }
        }
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

    public ExpType GetExpType()
    {
        return ExpType.ETPrim;
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

    public void Visit(List<Instruction> code)
    {
        foreach (var kv in kvs)
        {
            code.Add(new Instruction(OpCode.Push) { OpVar = kv.Key });
            kv.Value.Visit(code);
        }
        code.Add(new Instruction(OpCode.NewObj) { OpInt = kvs.Count });
    }
}

