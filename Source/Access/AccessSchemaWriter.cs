using AdFactum.Data.Internal;

namespace AdFactum.Data.Access
{
    public class AccessSchemaWriter : BaseSchemaWriter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeMapper"></param>
        /// <param name="databaseSchema"></param>
        public AccessSchemaWriter(ITypeMapper typeMapper, string databaseSchema) 
            : base(typeMapper, databaseSchema)
        {
        }


        /// <summary>
        /// Gets the field description for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        protected override string GetFieldDescriptionForDDL(FieldDescription field)
        {
            var result = field.IsAutoIncrement
                                ? string.Concat(TypeMapper.Quote(field.Name), " ", TypeMapper.AutoIncrementIdentifier)
                                : base.GetFieldDescriptionForDDL(field);

            return result;
        }
    }
}
