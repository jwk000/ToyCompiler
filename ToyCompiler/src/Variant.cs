using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{
    enum VariantType
    {
        Null,
        Number,
        String,
        Boolean,
        Array,
        Object,
        Function
    }

    class Variant
    {
        public VariantType variantType;
        public string id;
        public double num;
        public string str;
        public bool bol;
        public VArray arr;
        public VObject obj;
        public FunStat fun;

        public void Assign(Variant w)//赋值不包含id
        {
            this.variantType = w.variantType;
            this.num = w.num;
            this.str = w.str;
            this.bol = w.bol;
            this.arr = w.arr;
            this.obj = w.obj;
            this.fun = w.fun;
        }

        public override string ToString()
        {
            return variantType switch
            {
                VariantType.Null => $"null",
                VariantType.Boolean => $"{bol}",
                VariantType.Number => $"{num}",
                VariantType.String => $"{str}",
                VariantType.Array=>$"{arr}",
                VariantType.Object=>$"{obj}",
                VariantType.Function=>$"{fun}",
                _ => $"NotVariant"
            };
        }

        #region 类型转换

        public static implicit operator Variant(double d)
        {
            return new Variant { variantType = VariantType.Number, num = d };
        }

        public static implicit operator Variant(string s)
        {
            return new Variant { variantType = VariantType.String, str = s };
        }

        public static implicit operator Variant(bool b)
        {
            return new Variant { variantType = VariantType.Boolean, bol = b };
        }


        public static explicit operator double(Variant v)
        {
            if (v.variantType == VariantType.Number)
            {
                return v.num;
            }

            throw new InvalidCastException("variant is not a number");
        }

        public static explicit operator string(Variant v)
        {
            if (v.variantType == VariantType.String)
            {
                return v.str;
            }

            throw new InvalidCastException("variant is not a string");
        }

        public static explicit operator bool(Variant v)
        {
            if (v.variantType == VariantType.Boolean)
            {
                return v.bol;
            }
            throw new InvalidCastException("variant is not boolean");
        }

        #endregion

        #region 算术运算

        public static Variant operator +(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType)
            {
                throw new Exception("variant + not same type");
            }
            Variant v = new Variant();
            v.variantType = rhs.variantType;
            if (v.variantType == VariantType.Number)
            {
                v.num = lhs.num + rhs.num;
            }
            else if (v.variantType == VariantType.String)
            {
                v.str = lhs.str + rhs.str;
            }
            else
            {
                throw new Exception("variant null not support +");
            }
            return v;
        }

        public static Variant operator -(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType)
            {
                throw new Exception("variant - not same type");
            }
            Variant v = new Variant();
            v.variantType = rhs.variantType;
            if (v.variantType == VariantType.Number)
            {
                v.num = lhs.num - rhs.num;
            }
            else
            {
                throw new Exception("variant string not support -");
            }
            return v;
        }

        public static Variant operator *(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType)
            {
                throw new Exception("variant - not same type");
            }
            Variant v = new Variant();
            v.variantType = rhs.variantType;
            if (v.variantType == VariantType.Number)
            {
                v.num = lhs.num * rhs.num;
            }
            else
            {
                throw new Exception("variant string not support *");
            }
            return v;
        }

        public static Variant operator /(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType)
            {
                throw new Exception("variant - not same type");
            }
            Variant v = new Variant();
            v.variantType = rhs.variantType;
            if (v.variantType == VariantType.Number)
            {
                v.num = lhs.num / rhs.num;
            }
            else
            {
                throw new Exception("variant string not support /");
            }
            return v;
        }

        public static Variant operator ++(Variant v)
        {
            if (v.variantType == VariantType.Number)
            {
                v.num++;
            }
            else
            {
                throw new Exception("variant string not support ++");
            }
            return v;
        }

        public static Variant operator --(Variant v)
        {
            if (v.variantType == VariantType.Number)
            {
                v.num--;
            }
            else
            {
                throw new Exception("variant string not support --");
            }
            return v;
        }
        #endregion

        #region 比较操作
        public static bool operator ==(Variant lhs, Variant rhs)
        {
            if(lhs is null && rhs is null)
            {
                return true;
            }
            if (lhs is null || rhs is null)
            {
                return false;
            }
            if (lhs.variantType == rhs.variantType)
            {
                if (lhs.variantType == VariantType.Number)
                {
                    return lhs.num == rhs.num;
                }
                else if (lhs.variantType == VariantType.String)
                {
                    return lhs.str == rhs.str;
                }
                else if (lhs.variantType == VariantType.Boolean)
                {
                    return lhs.bol == rhs.bol;
                }
                else if (lhs.variantType == VariantType.Null)
                {
                    return true;
                }
                else if(lhs.variantType == VariantType.Array)
                {
                    return lhs.arr == rhs.arr;
                }
                else if(lhs.variantType == VariantType.Object)
                {
                    return lhs.obj == rhs.obj;
                }
                else if(lhs.variantType == VariantType.Function)
                {
                    return lhs.fun == rhs.fun;
                }
            }
            return false;
        }

        public static bool operator !=(Variant lhs, Variant rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator <(Variant lhs, Variant rhs)
        {
            if (lhs.variantType == rhs.variantType)
            {
                if (lhs.variantType == VariantType.Number)
                {
                    return lhs.num < rhs.num;
                }
            }
            throw new NotSupportedException($"{lhs} < {rhs}");

        }

        public static bool operator >(Variant lhs, Variant rhs)
        {
            if (lhs.variantType == rhs.variantType)
            {
                if (lhs.variantType == VariantType.Number)
                {
                    return lhs.num > rhs.num;
                }
            }
            throw new NotSupportedException($"{lhs} > {rhs}");

        }

        public static bool operator <=(Variant lhs, Variant rhs)
        {
            if (lhs.variantType == rhs.variantType)
            {
                if (lhs.variantType == VariantType.Number)
                {
                    return lhs.num <= rhs.num;
                }
            }
            throw new NotSupportedException($"{lhs} <= {rhs}");

        }

        public static bool operator >=(Variant lhs, Variant rhs)
        {
            if (lhs.variantType == rhs.variantType)
            {
                if (lhs.variantType == VariantType.Number)
                {
                    return lhs.num >= rhs.num;
                }
            }
            throw new NotSupportedException($"{lhs} >= {rhs}");

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        #endregion


    }

    interface ILength
    {
        double Length();
    }


    class VArray 
    {
        List<Variant> mArray = new List<Variant>();

        public Variant GetAt(int idx)
        {
            if(idx>=0 && idx < mArray.Count)
            {

                return mArray[idx];
            }
            return null;
        }

        public void Add(Variant v)
        {
            mArray.Add(v);
        }

        public double Length()
        {
            return mArray.Count;
        }

        public IEnumerator<Variant> GetEnumerator()
        {
            return mArray.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach(var v in mArray)
            {
                sb.Append(v).Append(',');
            }
            sb.Length--;
            sb.Append(']');
            return sb.ToString();
        }
    }

    class VObject
    {
        Dictionary<string, Variant> mDict = new Dictionary<string, Variant>();

        public Variant Get(string id)
        {
            return mDict[id];
        }

        public void Add(string id, Variant v)
        {
            mDict.Add(id, v);
        }

        public IEnumerator<KeyValuePair<string,Variant>> GetEnumerator()
        {
            return mDict.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            foreach (var v in mDict)
            {
                sb.Append(v).Append(',');
            }
            sb.Length--;
            sb.Append('}');
            return sb.ToString();
        }

    }


    //变量作用域
    class Scope
    {
        Dictionary<string, Variant> mVarDict = new Dictionary<string, Variant>();
        Scope mUpScope;

        public void SetUpScope(Scope scope)
        {
            mUpScope = scope;
        }

        public Variant GetVariant(string name)
        {
            Variant v = null;
            if(mVarDict.TryGetValue(name,out v))
            {
                return v;
            }
            if (mUpScope != null)
            {
                return mUpScope.GetVariant(name);
            }
            return v;
        }

        public bool AddVariant(Variant v)
        {
            if (mVarDict.ContainsKey(v.id))
            {
                return false;
            }

            mVarDict.Add(v.id, v);
            return true;
        }


        public void Clear()
        {
            mVarDict.Clear();
            mUpScope = null;
        }
    }
}
