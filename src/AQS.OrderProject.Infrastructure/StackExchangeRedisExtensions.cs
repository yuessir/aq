using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FastMember;
using StackExchange.Redis;

namespace AQS.OrderProject.Infrastructure
{
    public static class StackExchangeRedisExtensions
    {
        /// <summary>
        /// Converts instance of an object to hash entry list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> ConvertToHashEntryList<T>(this T instance) where T : new()
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var members = accessor.GetMembers();

            foreach (var member in members)
            {
                if (member.IsDefined(typeof(IgnoreDataMemberAttribute)))
                {
                    continue;
                }

                var type = member.Type;

                if (!type.IsValueType)//ATM supports only value types
                {
                    if (type != typeof(string))
                    {
                        continue;
                    }
                }

                var underlyingType = Nullable.GetUnderlyingType(type);
                var effectiveType = underlyingType ?? type;

                var val = accessor[instance, member.Name];

                if (val != null)
                {
                    if (effectiveType == typeof(DateTime))
                    {
                        var date = (DateTime)val;
                        if (date.Kind == DateTimeKind.Utc)
                        {

                            yield return new HashEntry(member.Name, $"{date.Ticks}|UTC");
                        }
                        else
                        {
                            yield return new HashEntry(member.Name, $"{date.Ticks}|LOC");
                        }

                    }
                    else
                    {
                        yield return new HashEntry(member.Name, val.ToString());
                    }

                }
            }
        }

        /// <summary>
        /// Converts from hash entry list and create instance of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries">The entries returned from StackExchange.redis.</param>
        /// <returns>Instance of Type T </returns>
        public static T ConvertFromHashEntryList<T>(this IEnumerable<HashEntry> entries) where T : new()
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var instance = new T();
            var hashEntries = entries as HashEntry[] ?? entries.ToArray();
            var members = accessor.GetMembers();
            foreach (var member in members)
            {
                if (member.IsDefined(typeof(IgnoreDataMemberAttribute)))
                {
                    continue;
                }

                var type = member.Type;

                if (!type.IsValueType) //ATM supports only value types
                {
                    if (type != typeof(string))
                    {
                        continue;
                    }

                }

                var underlyingType = Nullable.GetUnderlyingType(type);
                var effectiveType = underlyingType ?? type;


                var member1 = member;
                var entry = hashEntries.FirstOrDefault(e => e.Name.ToString().Equals(member1.Name));

                if (entry.Equals(new HashEntry()))
                {
                    continue;
                }

                var value = entry.Value.ToString();

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (effectiveType == typeof(DateTime))
                {
                    if (value.EndsWith("|UTC"))
                    {
                        value = value.TrimEnd("|UTC".ToCharArray());
                        DateTime date;

                        long ticks;
                        if (long.TryParse(value, out ticks))
                        {
                            date = new DateTime(ticks);
                            accessor[instance, member.Name] = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                        }



                    }
                    else
                    {
                        value = value.TrimEnd("|LOC".ToCharArray());
                        DateTime date;
                        long ticks;
                        if (long.TryParse(value, out ticks))
                        {
                            date = new DateTime(ticks);
                            accessor[instance, member.Name] = DateTime.SpecifyKind(date, DateTimeKind.Local);
                        }
                    }

                }
                else
                {
                    accessor[instance, member.Name] = Convert.ChangeType(entry.Value.ToString(), member.Type);
                }
            }


            return instance;
        }
    }
}