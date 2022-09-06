using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{

    enum TokenType
    {
        TTNum, // 42
        TTPlus, // +
        TTMinus, // -
        TTMultiple, // *
        TTDivision, // /
        TTMold,// %
        TTLeftBracket, // (
        TTRightBracket, // )
        TTLessThan, // <
        TTLessEqual, // <=
        TTGreatThan, // >
        TTGreatEqual, // >=
        TTEqual, // ==
        TTNotEqual, // !=
        TTAssign, // =
        TTPlusAssign, // +=
        TTMinusAssign, // -=
        TTMultipleAssign,// *=
        TTDivisionAssign,// /=
        TTMoldAssign, // %=
        TTPlusPlus, //++
        TTMinusMinus,//--
        TTAnd, // &&
        TTOr, // ||
        TTNot, // !
        TTBitAnd,// &
        TTBitOr, // |
        TTBitNot,// ~
        TTBitXor,// ^
        TTLeftShift,// <<
        TTRightShift,// >>
        TTBitAndAssign,// &=
        TTBitOrAssign,// |=
        TTBitXorAssign,// ^=
        TTLeftBracket2,// [
        TTRightBracket2,// ]
        TTLeftBracket3,// {
        TTRightBracket3,// }
        TTIf, // if
        TTElse,// else
        TTElif,// elif
        TTFor,// for
        TTWhile,// while
        TTBreak,// break
        TTContinue,//continue
        TTReturn, //return
        TTSimicolon,// ;
        TTComma,// ,
        TTColon, // :
        TTQuestion,// ?
        TTString,// "asdf"
        TTBool,// true false
        TTComment, // //
        TTID, // _id_
        TTFuncion, // function
        TTVar, // var
        TTDot, // .
        TTIn, // in
    }

    enum TokenState
    {
        None,
        NumInteger,
        NumDot,
        NumFloat,
        String,
        ID,
        Comment
    }


    class Token
    {
        public TokenType tokenType;
        public string desc;

        public override string ToString()
        {
            return $"<{tokenType}>{desc}";
        }

        public static string GetTokenString(List<Token> tokens)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var tk in tokens)
            {
                sb.Append(tk.desc);
            }
            return sb.ToString();
        }
    }

    internal class Lexer
    {
        public List<Token> mTokenList = new List<Token>();

        //先判断状态再判断输入
        public bool ParseToken(string exp)
        {
            TokenState st = TokenState.None;
            Token tk = null;
            for (int i = 0; i < exp.Length; i++)
            {
                char c = '\0';
                char d = '\0';
                if (i < exp.Length)
                {
                    c = exp[i];
                }
                if (i + 1 < exp.Length)
                {
                    d = exp[i + 1];
                }

                if (st == TokenState.Comment)
                {
                    if (c == '\n')
                    {
                        st = TokenState.None;
                    }
                    else
                    {
                        tk.desc += c;
                    }
                }
                else if (st == TokenState.String)
                {
                    if (c == '"')
                    {
                        st = TokenState.None;
                    }
                    else
                    {
                        tk.desc += c;
                    }
                }
                else if (st == TokenState.NumInteger)
                {
                    if (char.IsDigit(c))
                    {
                        tk.desc += c;
                    }
                    else if (c == '.')
                    {
                        st = TokenState.NumDot;
                        tk.desc += c;
                    }
                    else
                    {
                        i--;
                        st = TokenState.None;
                    }
                }
                else if (st == TokenState.NumFloat)
                {
                    if (char.IsDigit(c))
                    {
                        tk.desc += c;
                    }
                    else
                    {
                        i--;
                        st = TokenState.None;
                    }
                }
                else if (st == TokenState.NumDot)
                {
                    if (char.IsDigit(c))
                    {
                        st = TokenState.NumFloat;
                        tk.desc += c;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (st == TokenState.ID)
                {
                    if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        tk.desc += c;
                    }
                    else
                    {
                        i--;
                        st = TokenState.None;
                    }
                }
                else if (st == TokenState.None) //none
                {
                    if (char.IsWhiteSpace(c)) continue;
                    if (c == '"')
                    {
                        st = TokenState.String;
                        tk = new Token { tokenType = TokenType.TTString };
                        mTokenList.Add(tk);
                    }
                    else if (char.IsDigit(c))
                    {

                        st = TokenState.NumInteger;
                        tk = new Token();
                        mTokenList.Add(tk);
                        tk.tokenType = TokenType.TTNum;
                        tk.desc += c;
                    }
                    else if (c == '<')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTLessEqual, desc = "<=" };
                        }
                        else if (d == '<')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTLeftShift, desc = "<<" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTLessThan, desc = "<" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '>')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTGreatEqual, desc = ">=" };
                        }
                        else if (d == '>')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTRightShift, desc = ">>" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTGreatThan, desc = ">" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '=')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTEqual, desc = "==" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTAssign, desc = "=" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '!')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTNotEqual, desc = "!=" };
                        }
                        else
                        {

                            tk = new Token { tokenType = TokenType.TTNot, desc = "!" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '+')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTPlusAssign, desc = "+=" };
                        }
                        else if (d == '+')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTPlusPlus, desc = "++" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTPlus, desc = "+" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '-')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTMinusAssign, desc = "-=" };
                        }
                        else if (d == '-')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTMinusMinus, desc = "--" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTMinus, desc = "-" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '*')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTMultipleAssign, desc = "*=" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTMultiple, desc = "*" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '%')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTMoldAssign, desc = "%=" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTMold, desc = "%" };
                        }
                        mTokenList.Add(tk);
                    }
                    else if (c == '/')
                    {
                        if (d == '/')
                        {
                            st = TokenState.Comment;
                            i++;
                            tk = new Token { tokenType = TokenType.TTComment };
                        }
                        else if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTDivisionAssign, desc = "/=" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTDivision, desc = "/" };
                        }

                        //ignore comment
                        if (tk.tokenType != TokenType.TTComment)
                        {
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == '&')
                    {
                        if (d == '&')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTAnd, desc = "&&" };
                        }
                        else if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTBitAndAssign, desc = "&=" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTBitAnd, desc = "&" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '|')
                    {
                        if (d == '|')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTOr, desc = "||" };
                        }
                        else if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTBitOrAssign, desc = "|=" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTBitOr, desc = "|" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == '^')
                    {
                        if (d == '=')
                        {
                            i++;
                            tk = new Token { tokenType = TokenType.TTBitXorAssign, desc = "^=" };
                        }
                        else
                        {
                            tk = new Token { tokenType = TokenType.TTBitXor, desc = "^" };
                        }
                        mTokenList.Add(tk);

                    }
                    else if (c == 'i')
                    {
                        bool match = false;
                        if (d == 'f')
                        {
                            if (i + 2 < exp.Length && !char.IsLetterOrDigit(exp[i + 2]))
                            {
                                match = true;
                                i++;
                                tk = new Token { tokenType = TokenType.TTIf, desc = "if" };
                            }
                        }else if(d == 'n')
                        {
                            if (i + 2 < exp.Length && !char.IsLetterOrDigit(exp[i + 2]))
                            {
                                match = true;
                                i++;
                                tk = new Token { tokenType = TokenType.TTIn, desc = "in" };
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 'e')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'l')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 's')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'e')
                                {
                                    if (i + 4 < exp.Length && !char.IsLetterOrDigit(exp[i + 4]))
                                    {
                                        match = true;
                                        i += 3;
                                        tk = new Token { tokenType = TokenType.TTElse, desc = "else" };
                                    }
                                }
                            }
                            else if (i + 2 < exp.Length && exp[i + 2] == 'i')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'f')
                                {
                                    if (i + 4 < exp.Length && !char.IsLetterOrDigit(exp[i + 4]))
                                    {
                                        match = true;
                                        i += 3;
                                        tk = new Token { tokenType = TokenType.TTElif, desc = "elif" };
                                    }
                                }
                            }
                        }

                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 'f')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'o')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'r')
                            {
                                if (i + 3 < exp.Length && !char.IsLetterOrDigit(exp[i + 3]))
                                {
                                    match = true;
                                    i += 2;
                                    tk = new Token { tokenType = TokenType.TTFor, desc = "for" };
                                }
                            }
                        }
                        else if (i + 1 < exp.Length && exp[i + 1] == 'a')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'l')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 's')
                                {
                                    if (i + 4 < exp.Length && exp[i + 4] == 'e')
                                    {
                                        if (i + 5 < exp.Length && !char.IsLetterOrDigit(exp[i + 5]))
                                        {
                                            match = true;
                                            i += 4;
                                            tk = new Token { tokenType = TokenType.TTBool, desc = "false" };
                                        }
                                    }
                                }
                            }
                        }
                        else  if (i + 1 < exp.Length && exp[i + 1] == 'u')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'n')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'c')
                                {
                                    if (i + 4 < exp.Length && exp[i + 4] == 't')
                                    {
                                        if (i + 5 < exp.Length && exp[i + 5] == 'i')
                                        {
                                            if (i + 6 < exp.Length && exp[i + 6] == 'o')
                                            {
                                                if (i + 7 < exp.Length && exp[i + 7] == 'n')
                                                {
                                                    if (i + 8 < exp.Length && !char.IsLetterOrDigit(exp[i + 8]))
                                                    {
                                                        match = true;
                                                        i += 7;
                                                        tk = new Token { tokenType = TokenType.TTFuncion, desc = "function" };
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 'w')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'h')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'i')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'l')
                                {
                                    if (i + 4 < exp.Length && exp[i + 4] == 'e')
                                    {
                                        if (i + 5 < exp.Length && !char.IsLetterOrDigit(exp[i + 5]))
                                        {
                                            match = true;
                                            i += 4;
                                            tk = new Token { tokenType = TokenType.TTWhile, desc = "while" };
                                        }
                                    }
                                }
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 'b')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'r')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'e')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'a')
                                {
                                    if (i + 4 < exp.Length && exp[i + 4] == 'k')
                                    {
                                        if (i + 5 < exp.Length && !char.IsLetterOrDigit(exp[i + 5]))
                                        {
                                            match = true;
                                            i += 4;
                                            tk = new Token { tokenType = TokenType.TTBreak, desc = "break" };
                                        }
                                    }
                                }
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 'c')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'o')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'n')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 't')
                                {
                                    if (i + 4 < exp.Length && exp[i + 4] == 'i')
                                    {
                                        if (i + 5 < exp.Length && exp[i + 5] == 'n')
                                        {
                                            if (i + 6 < exp.Length && exp[i + 6] == 'u')
                                            {
                                                if (i + 7 < exp.Length && exp[i + 7] == 'e')
                                                {
                                                    if (i + 8 < exp.Length && !char.IsLetterOrDigit(exp[i + 8]))
                                                    {

                                                        match = true;
                                                        i += 7;
                                                        tk = new Token { tokenType = TokenType.TTContinue, desc = "continue" };
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 'r')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'e')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 't')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'u')
                                {
                                    if (i + 4 < exp.Length && exp[i + 4] == 'r')
                                    {
                                        if (i + 5 < exp.Length && exp[i + 5] == 'n')
                                        {
                                            if (i + 6 < exp.Length && !char.IsLetterOrDigit(exp[i + 6]))
                                            {

                                                match = true;
                                                i += 5;
                                                tk = new Token { tokenType = TokenType.TTReturn, desc = "return" };
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }

                    }
                    else if (c == 't')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'r')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'u')
                            {
                                if (i + 3 < exp.Length && exp[i + 3] == 'e')
                                {
                                    if (i + 4 < exp.Length && !char.IsLetterOrDigit(exp[i + 4]))
                                    {
                                        match = true;
                                        i += 3;
                                        tk = new Token { tokenType = TokenType.TTBool, desc = "true" };
                                    }
                                }
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }
                    }
                    else if (c == 'v')
                    {
                        bool match = false;
                        if (i + 1 < exp.Length && exp[i + 1] == 'a')
                        {
                            if (i + 2 < exp.Length && exp[i + 2] == 'r')
                            {
                                
                                    if (i + 3 < exp.Length && !char.IsLetterOrDigit(exp[i + 3]))
                                    {
                                        match = true;
                                        i += 2;
                                        tk = new Token { tokenType = TokenType.TTVar, desc = "var" };
                                    }
                            }
                        }
                        if (match)
                        {
                            mTokenList.Add(tk);
                        }
                        else
                        {
                            st = TokenState.ID;
                            tk = new Token { tokenType = TokenType.TTID };
                            tk.desc += c;
                            mTokenList.Add(tk);
                        }
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        st = TokenState.ID;
                        tk = new Token { tokenType = TokenType.TTID };
                        tk.desc += c;
                        mTokenList.Add(tk);
                    }

                    else
                    {
                        tk = c switch
                        {
                            '(' => new Token { tokenType = TokenType.TTLeftBracket, desc = c.ToString() },
                            ')' => new Token { tokenType = TokenType.TTRightBracket, desc = c.ToString() },
                            '[' => new Token { tokenType = TokenType.TTLeftBracket2, desc = c.ToString() },
                            ']' => new Token { tokenType = TokenType.TTRightBracket2, desc = c.ToString() },
                            '{' => new Token { tokenType = TokenType.TTLeftBracket3, desc = c.ToString() },
                            '}' => new Token { tokenType = TokenType.TTRightBracket3, desc = c.ToString() },
                            ':' => new Token { tokenType = TokenType.TTColon, desc = c.ToString() },
                            ';' => new Token { tokenType = TokenType.TTSimicolon, desc = c.ToString() },
                            ',' => new Token { tokenType = TokenType.TTComma, desc = c.ToString() },
                            '?' => new Token { tokenType = TokenType.TTQuestion, desc = c.ToString() },
                            '.' => new Token { tokenType = TokenType.TTDot, desc = c.ToString()},
                            _ => null
                        };

                        if (tk == null)
                        {
                            Console.WriteLine("invalid token at {0}", c);
                            return false;
                        }
                        mTokenList.Add(tk);

                    }
                }
            }

            if (st != TokenState.None)
            {
                Console.WriteLine("invalid end!");
                return false;
            }
            return true;
        }

        public string ShowTokens()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var token in mTokenList)
            {
                sb.AppendLine(token.ToString());
            }
            return sb.ToString();
        }

    }
}
