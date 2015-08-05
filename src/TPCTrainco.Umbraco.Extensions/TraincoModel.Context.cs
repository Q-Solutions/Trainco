﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TPCTrainco.Umbraco.Extensions
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class ATI_DevelopmentEntities1 : DbContext
    {
        public ATI_DevelopmentEntities1()
            : base("name=ATI_DevelopmentEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CATEGORY> CATEGORies { get; set; }
        public virtual DbSet<CATEGORYTYPE> CATEGORYTYPEs { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<CourseFormat> CourseFormats { get; set; }
        public virtual DbSet<COURS> COURSES { get; set; }
        public virtual DbSet<CourseTopic> CourseTopics { get; set; }
        public virtual DbSet<CourseType> CourseTypes { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationType> LocationTypes { get; set; }
        public virtual DbSet<OnSite> OnSites { get; set; }
        public virtual DbSet<ProductType> ProductTypes { get; set; }
        public virtual DbSet<RegBillCCType> RegBillCCTypes { get; set; }
        public virtual DbSet<RegBillPaymentMethodType> RegBillPaymentMethodTypes { get; set; }
        public virtual DbSet<RegContactMethodType> RegContactMethodTypes { get; set; }
        public virtual DbSet<RegistrationAttendee> RegistrationAttendees { get; set; }
        public virtual DbSet<RegistrationAttendeeSchedule> RegistrationAttendeeSchedules { get; set; }
        public virtual DbSet<REGISTRATION> REGISTRATIONS { get; set; }
        public virtual DbSet<SCHEDULE> SCHEDULES { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<ZipLookup> ZipLookups { get; set; }
        public virtual DbSet<INSTRUCTOR> INSTRUCTORS { get; set; }
        public virtual DbSet<SearchList_Category> SearchList_Category { get; set; }
        public virtual DbSet<SearchList_Locations> SearchList_Locations { get; set; }
        public virtual DbSet<SearchList_RegLocation> SearchList_RegLocation { get; set; }
        public virtual DbSet<Seminar_Catalog> Seminar_Catalog { get; set; }
        public virtual DbSet<ScheduleCourseInstructor> ScheduleCourseInstructors { get; set; }
    
        public virtual ObjectResult<Nullable<int>> add_Registration(Nullable<int> cartID)
        {
            var cartIDParameter = cartID.HasValue ?
                new ObjectParameter("cartID", cartID) :
                new ObjectParameter("cartID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("add_Registration", cartIDParameter);
        }
    }
}