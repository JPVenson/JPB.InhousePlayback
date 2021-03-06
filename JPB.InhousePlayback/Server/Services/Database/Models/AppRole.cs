﻿//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by a tool.
// Runtime Version:2.0.50727.42
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess;
using JPB.DataAccess.ModelsAnotations;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.EntityCollections;

namespace JPB.InhousePlayback.Server.Services.Database.Models
{


	[GeneratedCodeAttribute(tool: "JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator", version: "2.0.0.0")]
	public partial class AppRole
	{
		public AppRole() {}
		public int AppRoleId  { get; set; }
		public string RoleName  { get; set; }
		public string NormRoleName  { get; set; }
		public virtual DbCollection<AppUser> AppUser  { get; set; }
		public static AppRole Factory(EagarDataRecord reader)
		{
			var super = new AppRole();
			super.AppRoleId = (int)reader["AppRoleId"];
			super.RoleName = (string)reader["RoleName"];
			super.NormRoleName = (string)reader["NormRoleName"];
			var readersOfAppUser = ((EagarDataRecord[])reader["AppUser"]);
			super.AppUser = readersOfAppUser == null ? null : new DbCollection<AppUser>(readersOfAppUser.Select(item => ((AppUser)(typeof(AppUser).GetClassInfo().SetPropertiesViaReflection(reader: item)))));
			return super;
		}
		static partial void BeforeConfig();
		static partial void AfterConfig();
		static partial void BeforeConfig(ConfigurationResolver<AppRole> config);
		static partial void AfterConfig(ConfigurationResolver<AppRole> config);
		[ConfigMehtodAttribute]
		public static void Configuration(ConfigurationResolver<AppRole> config)
		{
			BeforeConfig();
			BeforeConfig(config);
			config.SetPropertyAttribute(s => s.AppRoleId, new PrimaryKeyAttribute());
			config.SetPropertyAttribute(s => s.AppUser, new ForeignKeyAttribute(referenceKey: "IdRole", foreignKey: "AppRoleId"));
			AfterConfig(config);
			AfterConfig();
		}
	}
}
