// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a scalar property of an entity type.
    /// </summary>
    public interface IReadOnlyProperty : IReadOnlyPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this property belongs to.
        /// </summary>
        IReadOnlyEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets a value indicating whether this property can contain <see langword="null" />.
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        ///     Gets a value indicating when a value for this property will be generated by the database. Even when the
        ///     property is set to be generated by the database, EF may still attempt to save a specific value (rather than
        ///     having one generated by the database) when the entity is added and a value is assigned, or the property is
        ///     marked as modified for an existing entity. See <see cref="GetBeforeSaveBehavior" />
        ///     and <see cref="GetAfterSaveBehavior" /> for more information.
        /// </summary>
        ValueGenerated ValueGenerated { get; }

        /// <summary>
        ///     Gets a value indicating whether this property is used as a concurrency token. When a property is configured
        ///     as a concurrency token the value in the database will be checked when an instance of this entity type
        ///     is updated or deleted during <see cref="DbContext.SaveChanges()" /> to ensure it has not changed since
        ///     the instance was retrieved from the database. If it has changed, an exception will be thrown and the
        ///     changes will not be applied to the database.
        /// </summary>
        bool IsConcurrencyToken { get; }

        /// <summary>
        ///     Returns the <see cref="CoreTypeMapping" /> for the given property from a finalized model.
        /// </summary>
        /// <returns> The type mapping. </returns>
        CoreTypeMapping GetTypeMapping()
        {
            var mapping = FindTypeMapping();
            if (mapping == null)
            {
                throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetTypeMapping)));
            }

            return mapping;
        }

        /// <summary>
        ///     Returns the type mapping for this property.
        /// </summary>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        CoreTypeMapping? FindTypeMapping();

        /// <summary>
        ///     Gets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" />
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <returns> The maximum length, or <see langword="null" /> if none is defined. </returns>
        int? GetMaxLength();

        /// <summary>
        ///     Gets the precision of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of digits.
        /// </summary>
        /// <returns> The precision, or <see langword="null" /> if none is defined. </returns>
        int? GetPrecision();

        /// <summary>
        ///     Gets the scale of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of decimal places.
        /// </summary>
        /// <returns> The scale, or <see langword="null" /> if none is defined. </returns>
        int? GetScale();

        /// <summary>
        ///     Gets a value indicating whether or not the property can persist Unicode characters.
        /// </summary>
        /// <returns> The Unicode setting, or <see langword="null" /> if none is defined. </returns>
        bool? IsUnicode();

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether or not this property can be modified before the entity is
        ///         saved to the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a value is assigned to this property when it is in
        ///         the <see cref="EntityState.Added" /> state.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any value
        ///         set will be ignored when it is in the <see cref="EntityState.Added" /> state.
        ///     </para>
        /// </summary>
        /// <returns> The before save behavior for this property. </returns>
        PropertySaveBehavior GetBeforeSaveBehavior();

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether or not this property can be modified after the entity is
        ///         saved to the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Throw" />, then an exception
        ///         will be thrown if a new value is assigned to this property after the entity exists in the database.
        ///     </para>
        ///     <para>
        ///         If <see cref="PropertySaveBehavior.Ignore" />, then any modification to the
        ///         property value of an entity that already exists in the database will be ignored.
        ///     </para>
        /// </summary>
        /// <returns> The after save behavior for this property. </returns>
        PropertySaveBehavior GetAfterSaveBehavior();

        /// <summary>
        ///     Gets the factory that has been set to generate values for this property, if any.
        /// </summary>
        /// <returns> The factory, or <see langword="null" /> if no factory has been set. </returns>
        Func<IProperty, IEntityType, ValueGenerator>? GetValueGeneratorFactory();

        /// <summary>
        ///     Gets the custom <see cref="ValueConverter" /> set for this property.
        /// </summary>
        /// <returns> The converter, or <see langword="null" /> if none has been set. </returns>
        ValueConverter? GetValueConverter();

        /// <summary>
        ///     Gets the type that the property value will be converted to before being sent to the database provider.
        /// </summary>
        /// <returns> The provider type, or <see langword="null" /> if none has been set. </returns>
        Type? GetProviderClrType();

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <returns> The comparer, or <see langword="null" /> if none has been set. </returns>
        ValueComparer? GetValueComparer();

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use with keys for this property, or <see langword="null" /> if none is set.
        /// </summary>
        /// <returns> The comparer, or <see langword="null" /> if none has been set. </returns>
        ValueComparer? GetKeyValueComparer();

        /// <summary>
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <returns> The first associated principal property, or <see langword="null" /> if none exists. </returns>
        IReadOnlyProperty? FindFirstPrincipal()
        {
            foreach (var foreignKey in GetContainingForeignKeys())
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (this == foreignKey.Properties[propertyIndex])
                    {
                        return foreignKey.PrincipalKey.Properties[propertyIndex];
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <returns> The list of all associated principal properties including the given property. </returns>
        IReadOnlyList<IReadOnlyProperty> GetPrincipals()
        {
            var principals = new List<IReadOnlyProperty> { this };
            AddPrincipals(this, principals);
            return principals;
        }

        private static void AddPrincipals(IReadOnlyProperty property, List<IReadOnlyProperty> visited)
        {
            foreach (var foreignKey in property.GetContainingForeignKeys())
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property == foreignKey.Properties[propertyIndex])
                    {
                        var principal = foreignKey.PrincipalKey.Properties[propertyIndex];
                        if (!visited.Contains(principal))
                        {
                            visited.Add(principal);

                            AddPrincipals(principal, visited);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this property is used as a foreign key (or part of a composite foreign key).
        /// </summary>
        /// <returns> <see langword="true" /> if the property is used as a foreign key, otherwise <see langword="false" />. </returns>
        bool IsForeignKey();

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <returns> The foreign keys that use this property. </returns>
        IEnumerable<IReadOnlyForeignKey> GetContainingForeignKeys();

        /// <summary>
        ///     Gets a value indicating whether this property is used as an index (or part of a composite index).
        /// </summary>
        /// <returns> <see langword="true" /> if the property is used as an index, otherwise <see langword="false" />. </returns>
        bool IsIndex();

        /// <summary>
        ///     Gets a value indicating whether this property is used as a unique index (or part of a unique composite index).
        /// </summary>
        /// <returns> <see langword="true" /> if the property is used as an unique index, otherwise <see langword="false" />. </returns>
        bool IsUniqueIndex()
            => GetContainingIndexes().Any(e => e.IsUnique);

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <returns> The indexes that use this property. </returns>
        IEnumerable<IReadOnlyIndex> GetContainingIndexes();

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key (or part of a composite primary key).
        /// </summary>
        /// <returns> <see langword="true" /> if the property is used as the primary key, otherwise <see langword="false" />. </returns>
        bool IsPrimaryKey()
            => FindContainingPrimaryKey() != null;

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <returns> The primary that use this property, or <see langword="null" /> if it is not part of the primary key. </returns>
        IReadOnlyKey? FindContainingPrimaryKey();

        /// <summary>
        ///     Gets a value indicating whether this property is used as the primary key or alternate key
        ///     (or part of a composite primary or alternate key).
        /// </summary>
        /// <returns> <see langword="true" /> if the property is used as a key, otherwise <see langword="false" />. </returns>
        bool IsKey();

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <returns> The primary and alternate keys that use this property. </returns>
        IEnumerable<IReadOnlyKey> GetContainingKeys();

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append($"Property: {DeclaringEntityType.DisplayName()}.");
            }

            builder.Append(Name).Append(" (");

            var field = GetFieldName();
            if (field == null)
            {
                builder.Append("no field, ");
            }
            else if (!field.EndsWith(">k__BackingField", StringComparison.Ordinal))
            {
                builder.Append(field).Append(", ");
            }

            builder.Append(ClrType.ShortDisplayName()).Append(")");

            if (IsShadowProperty())
            {
                builder.Append(" Shadow");
            }

            if (IsIndexerProperty())
            {
                builder.Append(" Indexer");
            }

            if (!IsNullable)
            {
                builder.Append(" Required");
            }

            if (IsPrimaryKey())
            {
                builder.Append(" PK");
            }

            if (IsForeignKey())
            {
                builder.Append(" FK");
            }

            if (IsKey()
                && !IsPrimaryKey())
            {
                builder.Append(" AlternateKey");
            }

            if (IsIndex())
            {
                builder.Append(" Index");
            }

            if (IsConcurrencyToken)
            {
                builder.Append(" Concurrency");
            }

            if (GetBeforeSaveBehavior() != PropertySaveBehavior.Save)
            {
                builder.Append(" BeforeSave:").Append(GetBeforeSaveBehavior());
            }

            if (GetAfterSaveBehavior() != PropertySaveBehavior.Save)
            {
                builder.Append(" AfterSave:").Append(GetAfterSaveBehavior());
            }

            if (ValueGenerated != ValueGenerated.Never)
            {
                builder.Append(" ValueGenerated.").Append(ValueGenerated);
            }

            if (GetMaxLength() != null)
            {
                builder.Append(" MaxLength(").Append(GetMaxLength()).Append(")");
            }

            if (IsUnicode() == false)
            {
                builder.Append(" Ansi");
            }

            if (GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(GetPropertyAccessMode());
            }

            if ((options & MetadataDebugStringOptions.IncludePropertyIndexes) != 0
                && ((AnnotatableBase)this).IsReadOnly)
            {
                var indexes = ((IProperty)this).GetPropertyIndexes();
                builder.Append(' ').Append(indexes.Index);
                builder.Append(' ').Append(indexes.OriginalValueIndex);
                builder.Append(' ').Append(indexes.RelationshipIndex);
                builder.Append(' ').Append(indexes.ShadowIndex);
                builder.Append(' ').Append(indexes.StoreGenerationIndex);
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
