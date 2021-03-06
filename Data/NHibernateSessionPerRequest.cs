﻿using System;
using System.Web;

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;

namespace DataAccess
{
    public class NHibernateSessionPerRequest : IHttpModule
    {
        private static readonly ISessionFactory sessionFactory;

        // Constructs our HTTP module
        static NHibernateSessionPerRequest()
        {
            sessionFactory = CreateSessionFactory();
        }

        // Initializes the HTTP module
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
        }

        // Disposes the HTTP module
        public void Dispose() { }

        // Returns the current session
        public static ISession GetCurrentSession()
        {
            return sessionFactory.GetCurrentSession();              
        }

        // Opens the session, begins the transaction, and binds the session
        private static void BeginRequest(object sender, EventArgs e)
        {
            ISession session = sessionFactory.OpenSession();
            session.BeginTransaction();
            CurrentSessionContext.Bind(session);
        }

        // Unbinds the session, commits the transaction, and closes the session
        private static void EndRequest(object sender, EventArgs e)
        {
            ISession session = CurrentSessionContext.Unbind(sessionFactory);

            if (session == null) return;

            try
            {
                session.Transaction.Commit();
            }
            catch (Exception)
            {
                session.Transaction.Rollback();
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
        }

        // Returns our session factory
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(CreateDbConfig)
                .Mappings(m => m.AutoMappings.Add(CreateMappings()))
                .ExposeConfiguration(UpdateSchema)
                .CurrentSessionContext<WebSessionContext>()
                .BuildSessionFactory();
        }

        // Returns our database configuration
        private static MsSqlConfiguration CreateDbConfig()
        {
            return MsSqlConfiguration
                .MsSql2008
                .ConnectionString(c => c.FromConnectionStringWithKey("DefaultConnection"));
        }

        // Returns our mappings
        private static AutoPersistenceModel CreateMappings()
        {
            return AutoMap
                .Assembly(System.Reflection.Assembly.GetCallingAssembly())
                .Where(t => t.Namespace != null && t.Namespace.EndsWith("Models"))
                .Conventions.Setup(c => c.Add(DefaultCascade.SaveUpdate()));
        }

        // Updates the database schema if there are any changes to the model,
        // or drops and creates it if it doesn't exist
        private static void UpdateSchema(Configuration cfg)
        {
            new SchemaUpdate(cfg)
                .Execute(false, true);
        }
    }
}