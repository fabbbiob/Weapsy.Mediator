﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using Weapsy.Mediator.EventStore.EF;

namespace Weapsy.Mediator.EventStore.EF.Migrations
{
    [DbContext(typeof(MediatorDbContext))]
    partial class MediatorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Weapsy.Mediator.EventStore.EF.Entities.AggregateEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.ToTable("DomainAggregate");
                });

            modelBuilder.Entity("Weapsy.Mediator.EventStore.EF.Entities.EventEntity", b =>
                {
                    b.Property<Guid>("AggregateId");

                    b.Property<int>("SequenceNumber");

                    b.Property<string>("Body");

                    b.Property<DateTime>("TimeStamp");

                    b.Property<string>("Type");

                    b.HasKey("AggregateId", "SequenceNumber");

                    b.ToTable("DomainEvent");
                });

            modelBuilder.Entity("Weapsy.Mediator.EventStore.EF.Entities.EventEntity", b =>
                {
                    b.HasOne("Weapsy.Mediator.EventStore.EF.Entities.AggregateEntity", "Aggregate")
                        .WithMany("Events")
                        .HasForeignKey("AggregateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
