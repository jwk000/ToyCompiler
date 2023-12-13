using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{
    class TokenReader
    {
        List<Token> mTokens;
        int mReadIndex = 0;
        
        public TokenReader(List<Token> tokens)
        {
            mTokens = tokens;
        }

        public List<Token> Tokens => mTokens;

        public static implicit operator TokenReader(List<Token> tokens)
        {
            return new TokenReader(tokens);
        }

        public bool IsEnd()
        {
            return mReadIndex >= mTokens.Count;
        }

        public Token Peek()
        {
            if (mReadIndex < mTokens.Count)
            {
                return mTokens[mReadIndex];
            }
            return null;
        }

        public Token Next()
        {
            if (mReadIndex < mTokens.Count)
            {
                return mTokens[mReadIndex++];
            }
            return null;
        }

        //有peek就不用rollback了
        public void Rollback(int n)
        {
            mReadIndex -= n;
        }


        public List<Token> SeekToEnd()
        {
            var ret = mTokens.Skip(mReadIndex).ToList();
            mReadIndex = mTokens.Count;
            return ret;
        }

        //tt会被吃掉
        public List<Token> SeekMatchBracket(TokenType ttleft, TokenType ttright)
        {
            List<Token> ret = new List<Token>();
            int depth = 0;
            Token t = Next();
            while (t != null)
            {
                if (t.tokenType == ttleft)
                {
                    depth++;
                }
                if (t.tokenType == ttright)
                {
                    depth--;
                }
                if (depth == 0)
                {
                    break;
                }
                ret.Add(t);
                t = Next();
            }
            return ret.Skip(1).ToList();
        }

        //tt会被吃掉
        public List<Token> SeekNextToken(TokenType tt)
        {
            List<Token> ret = new List<Token>();
            Token t = Next();
            while (t != null)
            {
                if (t.tokenType == tt)
                {
                    break;
                }
                ret.Add(t);
                t = Next();
            }
            return ret;
        }

        public bool ContainsToken(TokenType tt)
        {
            foreach(var t in mTokens)
            {
                if(t.tokenType == tt)
                {
                    return true;
                }
            }
            return false;
        }


    }

}
