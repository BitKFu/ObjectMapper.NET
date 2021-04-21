using System;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// Different types of alias
    /// </summary>
    public enum AliasType
    {
        /// <summary> Used to alias a SQL Select </summary>
        Select,

        /// <summary> Used to alias a column declaration </summary>
        Column,

        /// <summary> Used to alias a table definition </summary>
        Table,

        /// <summary> Used to alias an union definition </summary>
        Union,

        /// <summary> Used to alias a parameter defintion </summary>
        Parameter,

        /// <summary> Used to alias a join definitino </summary>
        Join
    }

    /// <summary>
    /// Alias Definition
    /// </summary>
    public sealed class Alias 
    {
        private string nonGeneratedAlias;
        private readonly string pre;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Alias"/> is generated.
        /// </summary>
        /// <value><c>true</c> if generated; otherwise, <c>false</c>.</value>
        public bool Generated
        {
            get { return nonGeneratedAlias == null; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return nonGeneratedAlias ?? pre + GetHashCode(); }
            set { nonGeneratedAlias = value;}
        }

        /// <summary>
        /// Generates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Alias Generate (AliasType type)
        {
            return new Alias(type);
        }

        /// <summary>
        /// Generates the specified real key.
        /// </summary>
        /// <param name="realAlias">The real alias.</param>
        /// <returns></returns>
        public static Alias Generate (string realAlias)
        {
            return new Alias(realAlias);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alias"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        private Alias(AliasType type)
        {
            switch (type)
            {
                case AliasType.Select:
                    pre = "S";
                    break;
                case AliasType.Column:
                    pre = "C";
                    break;
                case AliasType.Table:
                    pre = "T";
                    break;
                case AliasType.Parameter:
                    pre = "P";
                    break;
                case AliasType.Union:
                    pre = "U";
                    break;
                case AliasType.Join:
                    pre = "J";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alias"/> class.
        /// </summary>
        /// <param name="realAlias">The real alias.</param>
        private Alias(string realAlias)
        {
            nonGeneratedAlias = realAlias;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            Alias other = obj as Alias;
            if (other != null)
                return Name == other.Name && Generated == other.Generated && pre == other.pre;

            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}