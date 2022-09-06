using System;
using System.Collections.Generic;
using System.Linq;

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
}

class StatList : IStat
{
    public List<IStat> mStatList = new List<IStat>();
    public Scope mScope = new Scope();
    public StatCtrl Exec(Scope scope)
    {
        mScope.SetUpScope(scope);
        Env.LocalScope = mScope;
        foreach (var stat in mStatList)
        {
            StatCtrl ctrl = stat.Exec(mScope);
            if (ctrl != StatCtrl.None)
            {
                mScope.Clear();
                Env.LocalScope = scope;
                return ctrl;
            }
        }
        mScope.Clear();
        Env.LocalScope = scope;

        return StatCtrl.None;
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

    public bool Parse(TokenReader tokenReader)
    {
        var expTokens = tokenReader.SeekNextToken(TokenType.TTSimicolon);
        if (expTokens.Count == 0)
        {
            return false;
        }
        mExp = new Exp();
        return mExp.Parse(expTokens);
    }
}

class CompoundStat : IStat
{
    public List<IStat> mStatList = new List<IStat>();
    public Scope mScope = new Scope();
    public StatCtrl Exec(Scope scope)
    {
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
        mScope.Clear();
        Env.LocalScope = scope;
        return StatCtrl.None;
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

    public Scope mScope = new Scope();
    public StatCtrl Exec(Scope scope)
    {
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
            Env.RunList.Add(v);//TODO 多返回值就压多个
            return StatCtrl.Return;
        }
        else
        {
            throw new Exception($"{mToken} is not valid jump stat!");
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
    public Token mFunID;
    public List<Token> mParams;//形参
    public CompoundStat mStat;
    //public int mStackBaseIndex;//执行栈的基地址
    public Scope mScope = new Scope();//局部变量
    public Variant mVariant;
    public void Call(Scope scope)
    {
        //1 调用者把参数压栈
        //2 参数转换为局部变量
        //3 执行函数体
        //4 函数执行完清理参数
        //5 返回值压栈
        //6 调用者清理返回值

        mScope.SetUpScope(scope);
        Env.LocalScope = mScope;
        for (int i = 0; i < mParams.Count; i++)
        {
            if (i >= Env.RunList.Count)
            {
                break;
            }
            Token t = mParams[i];
            Variant v = Env.RunList[i];
            Variant l = new Variant();
            l.Assign(v);
            l.id = t.desc;
            mScope.AddVariant(l);
        }
        //清理多余的参数
        Env.RunList.Clear();
        if (mInnerBuild)
        {
            mInnerAction.Invoke();
        }
        else
        {
            mStat.Exec(mScope);
        }
        mScope.Clear();
        Env.LocalScope = scope;
    }

    public StatCtrl Exec(Scope scope)
    {
        mVariant = new Variant();
        mVariant.variantType = VariantType.Function;
        mVariant.id = mFunID.desc;
        mVariant.fun = this;
        scope.AddVariant(mVariant);
        return StatCtrl.None;
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
