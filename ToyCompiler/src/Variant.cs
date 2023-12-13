using System;
using System.Collections;
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
        Function,
        Label,
        Enum,
        Scope,
        Coroutine
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
        public Scope scope;
        public FunStat fun; //js实现的函数
        public APIDelegate api;//cs实现的函数
        public int label;//vm层的函数：vm跳转用的label
        public IEnumerator enu;//对象迭代器
        public Coroutine co;

        public void Assign(Variant w)//赋值不包含id
        {
            this.variantType = w.variantType;
            this.num = w.num;
            this.str = w.str;
            this.bol = w.bol;
            this.arr = w.arr;
            this.obj = w.obj;
            this.fun = w.fun;
            this.label = w.label;
            this.enu = w.enu;
            this.scope = w.scope;
            this.co = w.co;
        }

        public Variant Clone()
        {
            Variant v = new Variant();
            v.id=this.id;//连id都一样，作用域不同
            v.Assign(this); 
            return v;
        }

        public override string ToString()
        {
            return variantType switch
            {
                VariantType.Null => $"<null>",
                VariantType.Boolean => $"{bol}",
                VariantType.Number => $"{num}",
                VariantType.String => $"{str}",
                VariantType.Array => $"<arr>{arr}",
                VariantType.Object => $"<obj>{obj}",
                VariantType.Label => $"<label>{label}",
                VariantType.Function => $"<func>{id}",
                VariantType.Enum=>"<enum>",
                VariantType.Scope=>"<scope>",
                VariantType.Coroutine=>$"<co>{id}",
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
        public static implicit operator Variant(VArray a)
        {
            return new Variant { variantType = VariantType.Array, arr = a };
        }
        public static implicit operator Variant(VObject o)
        {
            return new Variant { variantType = VariantType.Object, obj = o };
        }
        public static implicit operator Variant(Scope s)
        {
            return new Variant { variantType = VariantType.Scope, scope = s };
        }
        public static implicit operator Variant(Coroutine c)
        {
            return new Variant { variantType = VariantType.Coroutine, co = c };
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

        public static explicit operator VArray (Variant v)
        {
            if(v.variantType == VariantType.Array)
            {
                return v.arr;
            }
            throw new InvalidCastException("variant is not array");
        }

        public static explicit operator VObject(Variant v)
        {
            if (v.variantType == VariantType.Object)
            {
                return v.obj;
            }
            throw new InvalidCastException("variant is not object");
        }

        public static explicit operator Scope(Variant v)
        {
            if (v.variantType == VariantType.Scope)
            {
                return v.scope;
            }
            throw new InvalidCastException("variant is not scope");
        }

        public static explicit operator Coroutine(Variant v)
        {
            if(v.variantType == VariantType.Coroutine)
            {
                return v.co;
            }
            throw new InvalidCastException("variant is not coroutine");
        }

        //重载true和false和&可以用&&操作
        public static bool operator true(Variant v)
        {
            if (v.variantType == VariantType.Boolean)
            {
                return v.bol;
            }
            if (v.variantType == VariantType.Number)
            {
                return v.num != 0;
            }
            return false;
        }

        public static bool operator false(Variant v)
        {
            if (v.variantType == VariantType.Boolean)
            {
                return !v.bol;
            }
            if (v.variantType == VariantType.Number)
            {
                return v.num == 0;
            }
            return false;
        }

        public static Variant operator &(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType && lhs.variantType != VariantType.Boolean)
            {
                throw new Exception("variant type not support operator &");
            }
            Variant v = new Variant();
            v.variantType = VariantType.Boolean;
            v.bol = lhs.bol & rhs.bol;
            return v;
        }

        public static Variant operator |(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType && lhs.variantType != VariantType.Boolean)
            {
                throw new Exception("variant type not support operator |");
            }
            Variant v = new Variant();
            v.variantType = VariantType.Boolean;
            v.bol = lhs.bol | rhs.bol;
            return v;
        }

        public static Variant operator !(Variant v)
        {
            if (v.variantType != VariantType.Boolean)
            {
                throw new Exception("variant type not support operator !");
            }
            Variant u = new Variant();
            u.variantType = VariantType.Boolean;
            u.bol = !v.bol;
            return v;
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

        public static Variant operator %(Variant lhs, Variant rhs)
        {
            if (lhs.variantType != rhs.variantType)
            {
                throw new Exception("variant - not same type");
            }
            Variant v = new Variant();
            v.variantType = rhs.variantType;
            if (v.variantType == VariantType.Number)
            {
                v.num = lhs.num % rhs.num;
            }
            else
            {
                throw new Exception("variant string not support %");
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
            if (lhs is null && rhs is null)
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
                    return Math.Abs(lhs.num - rhs.num)<0.0000001;
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
                else if (lhs.variantType == VariantType.Array)
                {
                    return lhs.arr == rhs.arr;
                }
                else if (lhs.variantType == VariantType.Object)
                {
                    return lhs.obj == rhs.obj;
                }
                else if (lhs.variantType == VariantType.Function)
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


    class VArray
    {
        List<Variant> mArray = new List<Variant>();

        public Variant GetAt(int idx)
        {
            if (idx >= 0 && idx < mArray.Count)
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

        public void Reverse()
        {
            mArray.Reverse();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            sb.AppendJoin(',', mArray);
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

        public IEnumerator<KeyValuePair<string, Variant>> GetEnumerator()
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

        public Scope GetUpScope()
        {
            return mUpScope;
        }

        public Variant GetVariant(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            Variant v = null;
            if (mVarDict.TryGetValue(name, out v))
            {
                return v;
            }
            if (mUpScope != null)
            {
                return mUpScope.GetVariant(name);
            }
            return v;
        }

        public void SetVariant(Variant v)
        {
            Variant findv = GetVariant(v.id);
            if(findv != null)
            {
                if(findv != v)
                {
                    throw new Exception($"variant {v.id} exist in this scope");
                }
                return;
            }
            
            mVarDict.Add(v.id, v);
        }

        public void Clear()
        {
            mVarDict.Clear();
            mUpScope = null;
        }

        public void Merge(Scope scope)
        {
            foreach(var v in scope.mVarDict.Values)
            {
                SetVariant(v);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach(var kv in mVarDict)
            {
                sb.AppendFormat("{0}:{1} ", kv.Key, kv.Value);
            }
            sb.Append("}");
            if(mUpScope != null)
            {
                sb.Append(" up:");
                sb.Append(mUpScope.ToString());
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
