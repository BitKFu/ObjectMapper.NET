using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;
using LINQPad.Extensibility.DataContext;

namespace ObjectMapper2LinqPad
{

    public class LinqPadConnection : DatabaseConnection
    {
        private readonly IConnectionInfo cxInfo;
        readonly XElement driverData;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqPadConnection"/> class.
        /// </summary>
        /// <param name="cxInfo">The cx info.</param>
        public LinqPadConnection(IConnectionInfo cxInfo)
        {
            this.cxInfo = cxInfo;
            driverData = cxInfo.DriverData;
        }

        /// <summary> Gets or sets a value indicating whether this <see cref="LinqPadConnection"/> is persist. </summary>
        public bool Persist
        {
            get { return CxInfo.Persist; }
            set { CxInfo.Persist = value; }
        }

        /// <summary> Gets or sets the database file. </summary>
        public override string DatabaseFile
        {
            get { return (string) driverData.Element("DatabaseFile") ?? string.Empty; }
            set { driverData.SetElementValue("DatabaseFile", value); }
        }

        /// <summary> Gets or sets the name of the database. </summary>
        public override string DatabaseName
        {
            get { return (string)driverData.Element("DatabaseName") ?? string.Empty; }
            set { driverData.SetElementValue("DatabaseName", value); }
        }

        /// <summary> Gets or sets the type of the database. </summary>
        public override DatabaseType DatabaseType
        {
            get { return (DatabaseType)Enum.Parse(typeof(DatabaseType), (string)driverData.Element("DatabaseType") ?? "SqlServer"); }
            set { driverData.SetElementValue("DatabaseType", value); }
        }

        /// <summary> Gets or sets the database version. </summary>
        public override double DatabaseVersion
        {
            get { return double.Parse((string)driverData.Element("DatabaseVersion") ?? string.Empty); }
            set { driverData.SetElementValue("DatabaseVersion", value); }
        }

        /// <summary> Gets or sets the data set. </summary>
        public override string DataSet
        {
            get { return (string)driverData.Element("DataSet") ?? string.Empty; }
            set { driverData.SetElementValue("DataSet", value); }
        }

        /// <summary> Gets or sets the db alias. </summary>
        public override string DbAlias
        {
            get { return (string)driverData.Element("DbAlias") ?? string.Empty; }
            set { driverData.SetElementValue("DbAlias", value); }
        }

        /// <summary> Gets or sets the name of the DSN. </summary>
        public override string DsnName
        {
            get { return (string)driverData.Element("DsnName") ?? string.Empty; }
            set { driverData.SetElementValue("DsnName", value); }
        }

        /// <summary> Gets or sets the password. </summary>
        public override string Password
        {
            get { return CxInfo.Decrypt ((string)driverData.Element("Password") ?? string.Empty); }
            set { driverData.SetElementValue("Password", CxInfo.Encrypt (value)); }
        }

        /// <summary> Gets or sets the physical database version. </summary>
        public override double PhysicalDatabaseVersion
        {
            get { return double.Parse((string)driverData.Element("PhysicalDatabaseVersion") ?? string.Empty); }
            set { driverData.SetElementValue("PhysicalDatabaseVersion", value); }
        }

        /// <summary> Gets or sets the name of the server. </summary>
        public override string ServerName
        {
            get { return (string)driverData.Element("ServerName") ?? string.Empty; }
            set { driverData.SetElementValue("ServerName", value); }
        }

        /// <summary> Gets or sets a value indicating whether the connection is a trusted connection. </summary>
        public override bool TrustedConnection
        {
            get { return bool.Parse((string)driverData.Element("TrustedConnection") ?? "false"); }
            set { driverData.SetElementValue("TrustedConnection", value); }
        }

        /// <summary> Gets or sets the name of the user. </summary>
        public override string UserName
        {
            get { return (string)driverData.Element("UserName") ?? string.Empty; }
            set { driverData.SetElementValue("UserName", value); }
        }

        /// <summary> Gets or sets the XML file. </summary>
        public override string XmlFile
        {
            get { return (string)driverData.Element("XmlFile") ?? string.Empty; }
            set { driverData.SetElementValue("XmlFile", value); }
        }

        /// <summary> Gets or sets the XSD file. </summary>
        public override string XsdFile
        {
            get { return (string)driverData.Element("XsdFile") ?? string.Empty; }
            set { driverData.SetElementValue("XsdFile", value); }
        }

        /// <summary> Defines the SQL Casing for this connection </summary>
        public override SqlCasing SqlCasing
        {
            get { return (SqlCasing)Enum.Parse(typeof(SqlCasing), (string)driverData.Element("SqlCasing") ?? "Mixed"); }
            set { driverData.SetElementValue("SqlCasing", value); }
        }

        /// <summary> Gets or sets the Database Schema </summary>
        public override string DatabaseSchema
        {
            get { return (string)driverData.Element("DatabaseSchema") ?? string.Empty; }
            set { driverData.SetElementValue("DatabaseSchema", value); }
        }

        /// <summary>
        /// Gets the cx info.
        /// </summary>
        /// <value>The cx info.</value>
        public IConnectionInfo CxInfo
        {
            get { return cxInfo; }
        }
    }
}
