using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFLib.Data.Attributes;
using Newtonsoft.Json;

namespace FFLib.Data
{
    /// <summary>
    /// Base Class for Database Entities
    /// </summary>
    public abstract class DBEntity<T> : FFLib.Data.ISupportsIsDirty
    {
        [NotPersisted]
        [Newtonsoft.Json.JsonIgnore]
        protected T _LoadedValues = default(T);

        public bool IsDirty()
        {
            return !FFLib.Data.DBEntityHelper.EntitiesAreEqual(this, this._LoadedValues);
        }

        /// <summary>
        /// Initializes the Clean State of the Entity. Call this function to initailize the clean state of the entity for use when determining IsDirty.
        /// </summary>
        public void InitCleanState()
        {
            this._LoadedValues = (T)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Base Class for Database Entities
    /// </summary>
    public class DBEntityHelper
    {
        /// <summary>
        /// Compares the serialized result of two objects for equality.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool EntitiesAreEqual(object obj1, object obj2)
        {
            if (obj1 == null & obj2 != null) return false;
            if (obj2 == null & obj1 != null) return false;
            if (obj1 == null & obj2 == null) return true;

            var serializedObject1 = Newtonsoft.Json.JsonConvert.SerializeObject(obj1, Formatting.Indented);
            var serializedObject2 = Newtonsoft.Json.JsonConvert.SerializeObject(obj2, Formatting.Indented);

            return serializedObject1 == serializedObject2;
        }

        /// <summary>
        /// Compares the serialized result of two objects for equality.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool EntitiesAreEqual<T>(T obj1, T obj2)
        {
            if (obj1 == null & obj2 != null) return true;
            if (obj2 == null & obj1 != null) return true;
            if (obj1 == null & obj2 == null) return false;

            var serializedObject1 = Newtonsoft.Json.JsonConvert.SerializeObject(obj1, Formatting.Indented);
            var serializedObject2 = Newtonsoft.Json.JsonConvert.SerializeObject(obj2, Formatting.Indented);

            return serializedObject1 == serializedObject2;
        }
    }
}
