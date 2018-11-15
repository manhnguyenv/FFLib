﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FFLib.Data.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            var jsonResolver = new IgnorePropertySerializerContractResolver();
            jsonResolver.IgnoreProperty(obj1.GetType(), "ModifiedDateUTC");
            jsonResolver.IgnoreProperty(obj1.GetType(), "ModifiedDate");
            jsonResolver.IgnoreProperty(obj1.GetType(), "ModifiedBy");
            if (obj1.GetType().FullName != obj2.GetType().FullName)
            {
                jsonResolver.IgnoreProperty(obj2.GetType(), "ModifiedDateUTC");
                jsonResolver.IgnoreProperty(obj2.GetType(), "ModifiedDate");
                jsonResolver.IgnoreProperty(obj2.GetType(), "ModifiedBy");
            }

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = jsonResolver,
                Converters = new List<JsonConverter> { new DecimalJsonConverter() }
            };

            var serializedObject1 = Newtonsoft.Json.JsonConvert.SerializeObject(obj1, Formatting.Indented, serializerSettings);
            var serializedObject2 = Newtonsoft.Json.JsonConvert.SerializeObject(obj2, Formatting.Indented, serializerSettings);

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

            var jsonResolver = new IgnorePropertySerializerContractResolver();
            jsonResolver.IgnoreProperty(typeof(T), "ModifiedDateUTC");
            jsonResolver.IgnoreProperty(typeof(T), "ModifiedDate");
            jsonResolver.IgnoreProperty(typeof(T), "ModifiedBy");

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = jsonResolver,
                Converters = new List<JsonConverter> { new DecimalJsonConverter() }
            };

            var serializedObject1 = Newtonsoft.Json.JsonConvert.SerializeObject(obj1, Formatting.Indented, serializerSettings);
            var serializedObject2 = Newtonsoft.Json.JsonConvert.SerializeObject(obj2, Formatting.Indented, serializerSettings);

            return serializedObject1 == serializedObject2;
        }

        public class IgnorePropertySerializerContractResolver : DefaultContractResolver
        {
            private readonly Dictionary<Type, HashSet<string>> _ignoredProperties;

            public IgnorePropertySerializerContractResolver()
            {
                _ignoredProperties = new Dictionary<Type, HashSet<string>>(10);
            }

            public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
            {
                if (!_ignoredProperties.ContainsKey(type))
                    _ignoredProperties[type] = new HashSet<string>();

                foreach (var p in jsonPropertyNames)
                    _ignoredProperties[type].Add(p);
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (IsIgnored(property.DeclaringType, property.PropertyName))
                {
                    property.ShouldSerialize = i => false;
                    property.Ignored = true;
                }

                return property;
            }

            private bool IsIgnored(Type type, string jsonPropertyName)
            {
                if (!_ignoredProperties.ContainsKey(type))
                    return false;

                return _ignoredProperties[type].Contains(jsonPropertyName);
            }

        }
    }

    public class DecimalJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// serialize all decimal values to have only a single trailing zero i.e. 1.000 = 1.0, 1.00 = 1.0, 1.100 = 1.10, 1.10 = 1.10, 1.0 = 1.0, 100 = 100
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) writer.WriteRawValue(null);
            else
            writer.WriteRawValue(((decimal)value).ToString("F8", CultureInfo.InvariantCulture).TrimEnd('0')+'0');
        }
    }
}
