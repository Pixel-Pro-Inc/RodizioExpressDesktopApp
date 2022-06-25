using RodizioSmartRestuarant.Interfaces;
using RodizioSmartRestuarant.Interfaces.BaseInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities.Aggregates
{
    /// <summary>
    /// This is a class that will define what all aggregates should have, eg Order, Menu
    /// </summary>
    public class BaseAggregates<Entity> : List<Entity>, IBaseAggregate
    {

        /// <summary>
        /// This is a property that is built depending on the type (in this case <typeparamref name="TValueType"/>) that you give it. The value it has depends on the elements of the aggregates within it
        /// <para>For example, if you have price. It will give you the total of all the constituents. But remember that it will only do this for specific type. You can't have a collection of float and a single float processed differently
        /// NOTE: It inherits from <see cref="BaseAggregates{T}"/> so it can get the abilities of it. It won't be able to get the abilities and properties of the subclasses of it.</para>
        /// <para> How is this any different from a regular property?</para>
        /// <para> This makes sure that you get the values from the <see cref="Entity.GetType()"/>s themselves and has checks  whether the aggregate is empty and on allowable types</para>
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        public class AggregateProp<TValueType>: BaseAggregates<Entity>
        {
            private TValueType _value;

            public TValueType Value
            {
                get
                {
                    // insert desired logic here
                    return _value;
                }
                set
                {
                    // If there are no elements in the aggregate you can't expect it to have values NOTE: We don't need to check if they are nullable. Cause if it isn't one of the types we want it will throw an error
                    if (!this.Any()) throw new NullReferenceException($"There are no {typeof(Entity)} in this {this}");

                    // Logic to check if it is NOT the right type and throws error
                    if (

                        ! (typeof(TValueType)==typeof(string) ^ 
                        typeof(TValueType) == typeof(DateTime) ^
                        typeof(TValueType) == typeof(float)) ^
                        typeof(TValueType) == typeof(int)^
                        typeof(TValueType) == typeof(bool)

                        ) { throw new InvalidCastException($"{this} currently does not have ability to handle this type. Please add logic in the type and edit the check logic to include it"); }

                    _value = value;

                }
            }

            public static implicit operator TValueType(AggregateProp<TValueType> property)
            {
                return property.Value;
            }

            public static implicit operator AggregateProp<TValueType>(TValueType value)
            {
                return new AggregateProp<TValueType> { Value = value };
            }
        }


    }
}
