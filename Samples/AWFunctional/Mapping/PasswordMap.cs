using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Infrastructure;

namespace AdventureWorksModel {
    public class PasswordMap : EntityTypeConfiguration<Password> {
        public PasswordMap() {
            // Primary Key
            HasKey(t => t.BusinessEntityID);

            // Table & Column Mappings
            this.ToTable("Password", "Person");
        }
    }
}