using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Extensions;
using SqlSugar;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = new string[] { "aa", "bb" };
            Expression<Func<UserInfo, bool>> exs1 = t => !str.Contains(t.FName);
            //!.Call System.Linq.Enumerable.Contains(
            //          .Constant < ConsoleApp3.Program +<> c__DisplayClass0_0 > (ConsoleApp3.Program +<> c__DisplayClass0_0).str,$t.FName)
            var sql = SqlBuilder.Select<UserInfo>().Where(exs1).Sql;
            Console.WriteLine(sql);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ParameterExpression parameterExpression = Expression.Parameter(typeof(UserInfo), "t");
            PropertyInfo propertyAny = typeof(UserInfo).GetProperty("FName");
            var propertyExpression = Expression.Property(parameterExpression, propertyAny);
            var value = new ExpressionTmpClass { Arr = str };
            var valueExpression = Expression.Constant(value);
            var handle = value.GetType().GetField("Arr").FieldHandle;
            FieldInfo fieldInfo = FieldInfo.GetFieldFromHandle(handle);
            var fieldExpression = Expression.Field(valueExpression, fieldInfo);
            var containMethod = typeof(Enumerable).GetMethods()
                               .Where(m => m.Name == "Contains")
                               .Single(m => m.GetParameters().Length == 2).MakeGenericMethod(typeof(string));

            //MethodInfo optionMethod = typeof(ObjectExtensions).GetMethod("In").MakeGenericMethod(typeof(string));
            //var call = Expression.Call(null, optionMethod, new Expression[2] { propertyExpression, fieldExpression });

            var callExpression = Expression.Call(null, containMethod, new Expression[2] { fieldExpression, propertyExpression });
            var notExpression = Expression.Not(callExpression);
            Expression<Func<UserInfo, bool>> exs2 = Expression.Lambda<Func<UserInfo, bool>>(notExpression, parameterExpression);
            //!.Call System.Linq.Enumerable.Contains(
            //          .Constant<ConsoleApp3.ExpressionTmpClass>(ConsoleApp3.ExpressionTmpClass).Arr,$t.FName)

            sql = SqlBuilder.Select<UserInfo>().Where(exs2).Sql;
            Console.WriteLine(sql);
            Console.WriteLine("//////////////////SqlSugar//////////////////////");

            using var dbSugar = new SqlSugarClient(new ConnectionConfig() { DbType = DbType.SqlServer });
            sql = dbSugar.Queryable<UserInfo>().Where(exs1).ToSql().Key;
            Console.WriteLine(sql);
            sql = dbSugar.Queryable<UserInfo>().Where(exs2).ToSql().Key;
            Console.WriteLine(sql);

            Console.WriteLine("//////////////////////但是//////////////////////");
            var right = Expression.Not(Expression.Call(containMethod, Expression.Constant(str),propertyExpression));
            Expression<Func<UserInfo, bool>> exs3= Expression.Lambda<Func<UserInfo, bool>>(right, parameterExpression);
            sql = dbSugar.Queryable<UserInfo>().Where(exs3).ToSql().Key;
            Console.WriteLine(sql);
            sql = SqlBuilder.Select<UserInfo>().Where(exs3).Sql;
            Console.WriteLine(sql);
            //SELECT * FROM UserInfo AS t WHERE t.FName NOT IN (@p__1,@p__2)
        }
    }

    public class ExpressionTmpClass
    {
        public string[] Arr;
    }
    public class UserInfo
    {
        public string FName { get; set; }
    }
}
