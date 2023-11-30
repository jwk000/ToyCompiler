using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace ToyCompiler;

/*
 * stat =  exp_stat | compound_stat | if_stat | while_stat | for_stat | jump_stat | fun_stat
 * exp_stat : exp ';' | ';'
 * compound_stat = '{' stat* '}'
 * if_stat = 'if' '(' exp ')' stat ('else' stat )?
 * while_stat = 'while' '(' exp ')' stat
 * for_stat = 'for' '(' exp? ';' exp? ';' exp? ')' stat | 'for' '(' exp 'in' exp ')' stat
 * jump_stat = 'break' ';'|'continue' ';' |'return' exp? ';'
 * fun_stat = 'function' ID '(' args ')' '{' stat* '}'
 * 
 */

enum StatType
{
    StatIf,
    StatFor,
    StatWhile,
    StatExp,
    StatCompound,
    StatJump,
    StatFun
}

enum StatCtrl
{
    None,
    Break,
    Continue,
    Return
}


interface IStat
{
    StatCtrl Exec(Scope scope);
    bool Parse(TokenReader tokenReader);
    void OnVisit(List<Instruction> code);
}

class StatList : IStat
{
    public List<IStat> mStatList = new List<IStat>();

    public StatCtrl Exec(Scope scope)
    {
        Env.LocalScope = scope;
        foreach (var stat in mStatList)
        {
            StatCtrl ctrl = stat.Exec(scope);
            if (ctrl != StatCtrl.None)
            {
                return ctrl;
            }
        }
        return StatCtrl.None;
    }

    public void OnVisit(List<Instruction> code)
    {
        foreach (var stat in mStatList)
        {
            stat.OnVisit(code);
        }
        //添加一个停机指令
        code.Add(new Instruction(OpCode.Halt));
    }

    public bool Parse(TokenReader tokenReader)
    {
        while (!tokenReader.IsEnd())
        {
            Stat stat = new Stat();
            if (stat.Parse(tokenReader))
            {
                mStatList.Add(stat);
            }
            else
            {
                return false;
            }

        }
        return true;
    }
}

class Stat : IStat
{
    public StatType mStatType;
    public ExpStat mExpStat;
    public CompoundStat mCompundStat;
    public IfStat mIfStat;
    public WhileStat mWhileStat;
    public ForStat mForStat;
    public JumpStat mJumpStat;
    public FunStat mFunStat;
    public StatCtrl Exec(Scope scope)
    {
        switch (mStatType)
        {
            case StatType.StatCompound:
                return mCompundStat.Exec(scope);
            case StatType.StatExp:
                return mExpStat.Exec(scope);
            case StatType.StatFor:
                return mForStat.Exec(scope);
            case StatType.StatIf:
                return mIfStat.Exec(scope);
            case StatType.StatWhile:
                return mWhileStat.Exec(scope);
            case StatType.StatJump:
                return mJumpStat.Exec(scope);
            case StatType.StatFun:
                return mFunStat.Exec(scope);
            default:
                break;
        }
        return StatCtrl.None;
    }

    public void OnVisit(List<Instruction> code)
    {
        switch (mStatType)
        {
            case StatType.StatCompound:
                mCompundStat.OnVisit(code); break;
            case StatType.StatExp:
                mExpStat.OnVisit(code); break;
            case StatType.StatFor:
                mForStat.OnVisit(code); break;
            case StatType.StatIf:
                mIfStat.OnVisit(code); break;
            case StatType.StatWhile:
                mWhileStat.OnVisit(code); break;
            case StatType.StatJump:
                mJumpStat.OnVisit(code); break;
            case StatType.StatFun:
                mFunStat.OnVisit(code); break;
            default:
                break;
        }
    }
    public bool Parse(TokenReader tokenReader)
    {
        Token t = tokenReader.Peek();

        if (t.tokenType == TokenType.TTIf)
        {
            mStatType = StatType.StatIf;
            mIfStat = new IfStat();
            return mIfStat.Parse(tokenReader);
        }
        else if (t.tokenType == TokenType.TTWhile)
        {
            mStatType = StatType.StatWhile;
            mWhileStat = new WhileStat();
            return mWhileStat.Parse(tokenReader);
        }
        else if (t.tokenType == TokenType.TTFor)
        {
            mStatType = StatType.StatFor;
            mForStat = new ForStat();
            return mForStat.Parse(tokenReader);
        }
        else if (t.tokenType == TokenType.TTBreak || t.tokenType == TokenType.TTContinue || t.tokenType == TokenType.TTReturn)
        {
            mStatType = StatType.StatJump;
            mJumpStat = new JumpStat();
            return mJumpStat.Parse(tokenReader);
        }
        else if (t.tokenType == TokenType.TTLeftBracket3)
        {
            mStatType = StatType.StatCompound;
            mCompundStat = new CompoundStat();
            return mCompundStat.Parse(tokenReader);
        }
        else if (t.tokenType == TokenType.TTFuncion)
        {
            mStatType = StatType.StatFun;
            mFunStat = new FunStat();
            return mFunStat.Parse(tokenReader);
        }
        else
        {
            mStatType = StatType.StatExp;
            mExpStat = new ExpStat();
            return mExpStat.Parse(tokenReader);
        }
    }
}

class ExpStat : IStat
{
    Exp mExp;
    public StatCtrl Exec(Scope scope)
    {
        mExp?.Calc();
        return StatCtrl.None;
    }
    public void OnVisit(List<Instruction> code)
    {
        //如果是个语句则执行后栈应该是空的
        mExp.OnVisit(code);
        mExp.MaybeAddClear(code);
    }
    public bool Parse(TokenReader tokenReader)
    {
        var expTokens = tokenReader.SeekNextToken(TokenType.TTSimicolon);
        if (expTokens.Count == 0)
        {
            return false;
        }
        mExp = new Exp();
        return mExp.Parse(expTokens) && IsValidExpStatement();
    }

    //只有赋值，声明，自增，函数调用可以作为语句存在
    bool IsValidExpStatement()
    {
        switch (mExp.GetExpType())
        {
            case ExpType.ETAssign:
            case ExpType.ETVarDecl:
            case ExpType.ETPreInc:
            case ExpType.ETPosInc:
            case ExpType.ETCall:
                return true;
            default:
                return false;
        }
    }
}

class CompoundStat : IStat
{
    public List<IStat> mStatList = new List<IStat>();
    public StatCtrl Exec(Scope scope)
    {
        Scope mScope = new Scope();
        mScope.SetUpScope(scope);
        Env.LocalScope = mScope;
        foreach (var stat in mStatList)
        {
            StatCtrl ctrl = stat.Exec(mScope);
            if (ctrl != StatCtrl.None)
            {
                Env.LocalScope = scope;
                return ctrl;
            }
        }
        Env.LocalScope = scope;
        return StatCtrl.None;
    }

    public void OnVisit(List<Instruction> code)
    {
        //嵌套scope不好处理break和continue
        //code.Add(new Instruction(OpCode.EnterScope));
        foreach (var stat in mStatList)
        {
            stat.OnVisit(code);
        }
        //code.Add(new Instruction(OpCode.LeaveScope));
    }

    public bool Parse(TokenReader tokenReader)
    {
        var tokens = tokenReader.SeekMatchBracket(TokenType.TTLeftBracket3, TokenType.TTRightBracket3);
        if (tokens.Count == 0)
        {
            return false;
        }
        TokenReader reader = new TokenReader(tokens);
        while (!reader.IsEnd())
        {
            Stat stat = new Stat();
            if (stat.Parse(reader))
            {
                mStatList.Add(stat);
            }
            else
            {
                return false;
            }

        }
        return true;
    }
}



class IfStat : IStat
{
    public Exp mCondExp;
    public IStat mIfStat;
    public IStat mElseStat;

    public StatCtrl Exec(Scope scope)
    {
        if (mCondExp.Calc() == true)
        {
            StatCtrl ctrl = mIfStat.Exec(scope);
            if (ctrl != StatCtrl.None)
            {
                return ctrl;
            }
        }
        else
        {
            if (mElseStat != null)
            {
                var ctrl = mElseStat.Exec(scope);
                if (ctrl != StatCtrl.None)
                {
                    return ctrl;
                }
            }
        }
        return StatCtrl.None;
    }

    public void OnVisit(List<Instruction> code)
    {
        mCondExp.OnVisit(code);
        Instruction njump = new Instruction(OpCode.JumpFalse);//跳过if
        code.Add(njump);
        mIfStat.OnVisit(code);
        Instruction jump = new Instruction(OpCode.Jump);//跳过else
        code.Add(jump);
        njump.OpInt = code.Count; //妙
        if (mElseStat != null)
        {
            mElseStat.OnVisit(code);
        }
        jump.OpInt = code.Count;
    }
    public bool Parse(TokenReader tokenReader)
    {

        var t = tokenReader.Next();
        if (t.tokenType != TokenType.TTIf)
        {
            return false;
        }
        t = tokenReader.Peek();
        if (t.tokenType != TokenType.TTLeftBracket)
        {
            return false;
        }
        var expTokens = tokenReader.SeekMatchBracket(TokenType.TTLeftBracket, TokenType.TTRightBracket);
        if (expTokens == null)
        {
            return false;
        }
        mCondExp = new Exp();
        if (!mCondExp.Parse(expTokens))
        {
            return false;
        }

        mIfStat = new Stat();
        if (!mIfStat.Parse(tokenReader))
        {
            return false;
        }

        t = tokenReader.Peek();
        if (t.tokenType == TokenType.TTElse)
        {
            tokenReader.Next();
            mElseStat = new Stat();
            if (!mElseStat.Parse(tokenReader))
            {
                return false;
            }
            return true;
        }
        else
        {
            return true;
        }
    }
}

enum ForStatType
{
    LoopFor,
    IterFor,
}
class ForStat : IStat
{
    public ForStatType mForStatType;
    public Exp mExp1;
    public Exp mExp2;
    public Exp mExp3;
    public IStat mStat;
    public ForInExp mForinExp;

    public StatCtrl Exec(Scope scope)
    {
        Scope mScope = new Scope();
        mScope.SetUpScope(scope);
        Env.LocalScope = mScope;
        if (mForStatType == ForStatType.LoopFor)
        {
            for (mExp1?.Calc(); mExp2 == null ? true : mExp2.Calc() == true; mExp3?.Calc())
            {
                var ctrl = mStat.Exec(mScope);
                if (ctrl == StatCtrl.Return)
                {
                    mScope.Clear();
                    Env.LocalScope = scope;
                    return StatCtrl.Return;
                }
                else if (ctrl == StatCtrl.Break)
                {
                    break;
                }
                else if (ctrl == StatCtrl.Continue)
                {
                    continue;
                }
            }
        }
        else if (mForStatType == ForStatType.IterFor)
        {
            mForinExp.Calc();
            while (mForinExp.Next())
            {
                var ctrl = mStat.Exec(mScope);
                if (ctrl == StatCtrl.Return)
                {
                    mScope.Clear();
                    Env.LocalScope = scope;
                    return StatCtrl.Return;
                }
                else if (ctrl == StatCtrl.Break)
                {
                    break;
                }
                else if (ctrl == StatCtrl.Continue)
                {
                    continue;
                }
            }

        }
        mScope.Clear();
        Env.LocalScope = scope;
        return StatCtrl.None;
    }
    public void OnVisit(List<Instruction> code)
    {

        code.Add(new Instruction(OpCode.EnterScope));
        if (mForStatType == ForStatType.LoopFor)
        {
            if (mExp1 != null)
            {
                mExp1.OnVisit(code);
                mExp1.MaybeAddClear(code);
            }
            //第一次不执行后处理
            Instruction firstJump = new Instruction(OpCode.Jump);
            code.Add(firstJump);

            //continue 从这里开始
            Instruction cjump = new Instruction(OpCode.Jump) { OpInt = code.Count };
            Instruction njump = new Instruction(OpCode.JumpFalse);
            Instruction bjump = new Instruction(OpCode.Jump);
            JumpLabel jumplabel = new JumpLabel(cjump, bjump);
            Instruction.JumpLabels.Push(jumplabel);

            //后处理
            if (mExp3 != null)
            {
                mExp3.OnVisit(code);
                mExp3.MaybeAddClear(code);
            }
            firstJump.OpInt = code.Count;

            //条件判断
            if (mExp2 == null)
            {
                code.Add(new Instruction(OpCode.Push) { OpVar = true });
            }
            else
            {
                mExp2.OnVisit(code);
            }
            code.Add(njump);
            //循环体
            mStat.OnVisit(code);

            code.Add(cjump);
            njump.OpInt = code.Count;
            bjump.OpInt = code.Count;
            Instruction.JumpLabels.Pop();
        }
        else if (mForStatType == ForStatType.IterFor)
        {
            mForinExp.OnVisit(code);
            //需要保证栈顶是exp返回的对象或数组
            //迭代器添加的两个变量压栈
            Variant key = new Variant();
            key.id = mForinExp.mParams[0].desc;
            code.Add(new Instruction(OpCode.Push) { OpVar = key });
            Variant val = new Variant();
            val.id = mForinExp.mParams[1].desc;
            code.Add(new Instruction(OpCode.Push) { OpVar = val });
            //迭代器
            code.Add(new Instruction(OpCode.Enum));
            //循环起点
            int label = code.Count;
            Instruction cjump = new Instruction(OpCode.Jump) { OpInt = label };
            Instruction njump = new Instruction(OpCode.JumpFalse);
            Instruction bjump = new Instruction(OpCode.Jump);
            JumpLabel jumplabel = new JumpLabel(cjump, bjump);
            Instruction.JumpLabels.Push(jumplabel);
            //通过next指令迭代
            Instruction next = new Instruction(OpCode.Next);
            code.Add(next);
            mStat.OnVisit(code);

            code.Add(cjump);
            next.OpInt = code.Count;//next完成后跳转到后面的指令
            njump.OpInt = code.Count;
            bjump.OpInt = code.Count;
            //支持多级break
            Instruction.JumpLabels.Pop();
        }

        code.Add(new Instruction(OpCode.LeaveScope));
    }
    public bool Parse(TokenReader tokenReader)
    {
        TokenReader expReader = null;
        var t = tokenReader.Next();

        //for

        if (t.tokenType != TokenType.TTFor)
        {
            return false;
        }

        t = tokenReader.Peek();

        // ()
        if (t.tokenType != TokenType.TTLeftBracket)
        {
            return false;
        }
        var expTokens = tokenReader.SeekMatchBracket(TokenType.TTLeftBracket, TokenType.TTRightBracket);
        expReader = new TokenReader(expTokens);
        if (expReader.ContainsToken(TokenType.TTIn))
        {
            mForStatType = ForStatType.IterFor;
            // for in exp
            mForinExp = new ForInExp();
            if (!mForinExp.Parse(expReader))
            {
                return false;
            }
        }
        else
        {
            mForStatType = ForStatType.LoopFor;
            // init exp
            var initExpTokens = expReader.SeekNextToken(TokenType.TTSimicolon);
            if (initExpTokens.Count == 0)
            {
                return false;
            }
            mExp1 = new Exp();
            if (!mExp1.Parse(initExpTokens))
            {
                return false;
            }

            // cond exp
            var condExpTokens = expReader.SeekNextToken(TokenType.TTSimicolon);
            if (condExpTokens.Count == 0)
            {
                return false;
            }
            mExp2 = new Exp();
            if (!mExp2.Parse(condExpTokens))
            {
                return false;
            }

            //loop exp
            var loopExpTokens = expReader.SeekToEnd();
            if (loopExpTokens.Count == 0)
            {
                return false;
            }
            mExp3 = new Exp();
            if (!mExp3.Parse(loopExpTokens))
            {
                return false;
            }

        }

        //stat
        mStat = new Stat();
        return mStat.Parse(tokenReader);
    }
}

class WhileStat : IStat
{
    public Exp mCondExp;
    public Stat mStat;
    public StatCtrl Exec(Scope scope)
    {
        while (mCondExp.Calc() == true)
        {
            var ctrl = mStat.Exec(scope);
            if (ctrl == StatCtrl.Return)
            {
                return StatCtrl.Return;
            }
            else if (ctrl == StatCtrl.Break)
            {
                break;
            }
            else if (ctrl == StatCtrl.Continue)
            {
                continue;
            }
        }
        return StatCtrl.None;
    }

    public void OnVisit(List<Instruction> code)
    {
        int label = code.Count;
        Instruction cjump = new Instruction(OpCode.Jump) { OpInt = label };
        Instruction njump = new Instruction(OpCode.JumpFalse);
        Instruction bjump = new Instruction(OpCode.Jump);
        JumpLabel jumplabel = new JumpLabel(cjump, bjump);
        Instruction.JumpLabels.Push(jumplabel);

        mCondExp.OnVisit(code);
        code.Add(njump);
        mStat.OnVisit(code);
        code.Add(cjump);
        njump.OpInt = code.Count;
        bjump.OpInt = code.Count;
        Instruction.JumpLabels.Pop();
    }

    public bool Parse(TokenReader tokenReader)
    {
        var t = tokenReader.Next();

        if (t.tokenType != TokenType.TTWhile)
        {
            return false;
        }
        t = tokenReader.Peek();

        if (t.tokenType != TokenType.TTLeftBracket)
        {
            return false;
        }

        var expTokens = tokenReader.SeekMatchBracket(TokenType.TTLeftBracket, TokenType.TTRightBracket);
        if (expTokens.Count == 0)
        {
            return false;
        }

        mCondExp = new Exp();
        if (!mCondExp.Parse(expTokens))
        {
            return false;
        }

        mStat = new Stat();
        return mStat.Parse(tokenReader);
    }
}

//break continue break
class JumpStat : IStat
{
    public Token mToken;
    public Exp mRetExp;
    public StatCtrl Exec(Scope scope)
    {
        if (mToken.tokenType == TokenType.TTBreak)
        {
            return StatCtrl.Break;
        }
        else if (mToken.tokenType == TokenType.TTContinue)
        {
            return StatCtrl.Continue;
        }
        else if (mToken.tokenType == TokenType.TTReturn)
        {
            var v = mRetExp.Calc();
            Env.RunStack.Add(v);//TODO 多返回值就压多个
            return StatCtrl.Return;
        }
        else
        {
            throw new Exception($"{mToken} is not valid jump stat!");
        }
    }

    public void OnVisit(List<Instruction> code)
    {
        if (mToken.tokenType == TokenType.TTBreak)
        {
            JumpLabel label = Instruction.JumpLabels.Peek();
            code.Add(label.BreakJump);
        }
        else if (mToken.tokenType == TokenType.TTContinue)
        {
            JumpLabel label = Instruction.JumpLabels.Peek();
            code.Add(label.ContinueJump);
        }
        else if (mToken.tokenType == TokenType.TTReturn)
        {
            mRetExp.OnVisit(code);
            code.Add(new Instruction(OpCode.Ret));
        }

    }

    public bool Parse(TokenReader tokenReader)
    {
        var t = tokenReader.Next();
        mToken = t;
        if (t.tokenType == TokenType.TTReturn)
        {
            mRetExp = new Exp();
            var expTokens = tokenReader.SeekNextToken(TokenType.TTSimicolon);
            if (!mRetExp.Parse(expTokens))
            {
                return false;
            }
        }
        else
        {
            tokenReader.SeekNextToken(TokenType.TTSimicolon);
        }

        return true;
    }
}


class FunStat : IStat
{
    public bool mInnerBuild;//内置函数
    public Action mInnerAction;
    public Action<List<Instruction>> mInnerVisit;

    public Token mFunID;
    public List<Token> mParams;//形参
    public CompoundStat mStat;


    public void Call(Scope scope)
    {
        //1 调用者把参数压栈
        //2 参数转换为局部变量
        //3 执行函数体
        //4 函数执行完清理参数
        //5 返回值压栈
        //6 调用者清理返回值

        //函数不应该访问上层作用域，实现闭包则copy一份
        //可以访问全局作用域
        Scope mScope = new Scope();
        mScope.SetUpScope(Env.GlobalScope);
        Env.LocalScope = mScope;
        for (int i = 0; i < mParams.Count; i++)
        {
            if (i >= Env.RunStack.Count)
            {
                break;
            }
            Token t = mParams[i];
            Variant v = Env.RunStack[i];
            Variant l = new Variant();
            l.Assign(v);
            l.id = t.desc;
            mScope.SetVariant(l);
        }
        //清理参数
        Env.RunStack.Clear();
        if (mInnerBuild)
        {
            mInnerAction.Invoke();
        }
        else
        {
            mStat.Exec(mScope);
        }
        mScope.Clear();
        Env.LocalScope = scope;//返回上层作用域
    }

    public StatCtrl Exec(Scope scope)
    {
        var v = new Variant();
        v.variantType = VariantType.Function;
        v.id = mFunID.desc;
        v.fun = this;
        scope.SetVariant(v);
        return StatCtrl.None;
    }

    public void OnVisit(List<Instruction> code)
    {
        var v = new Variant();
        v.variantType = VariantType.Function;
        v.id = mFunID.desc;
        v.fun = this;
        code.Add(new Instruction(OpCode.Push) { OpVar = v });
        code.Add(new Instruction(OpCode.Store));//栈空了
        Instruction jump = new Instruction(OpCode.Jump);//函数只能被调用，自己不执行
        code.Add(jump);
        v.label = code.Count;

        if (mInnerBuild)
        {
            mInnerVisit.Invoke(code);
        }
        else
        {
            mStat.OnVisit(code);
        }
        //返回值在栈顶，return语句清栈后写入返回值
        //手动return一下
        code.Add(new Instruction(OpCode.Ret));
        jump.OpInt = code.Count;
    }

    public bool Parse(TokenReader tokenReader)
    {
        var t = tokenReader.Next();

        if (t.tokenType != TokenType.TTFuncion)
        {
            return false;
        }

        t = tokenReader.Next();
        if (t.tokenType != TokenType.TTID)
        {
            return false;
        }
        mFunID = t;

        t = tokenReader.Peek();

        if (t.tokenType != TokenType.TTLeftBracket)
        {
            return false;
        }

        var expTokens = tokenReader.SeekMatchBracket(TokenType.TTLeftBracket, TokenType.TTRightBracket);
        if (expTokens.Count > 0)
        {
            mParams = new List<Token>();
            TokenReader tr = new TokenReader(expTokens);
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
        }

        mStat = new CompoundStat();
        bool ret = mStat.Parse(tokenReader);
        return ret;
    }

}
