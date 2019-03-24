﻿using System;
using GraphQL.SchemaGenerator.Tests.Mocks;
using GraphQL.SchemaGenerator.Tests.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GraphQL.SchemaGenerator.Tests.Tests
{
    [TestClass]
    public class ErrorTests
    {
        [TestMethod]
        public void Duplicate_Name_Throws()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            Exception e = null;
            try
            {
                schemaGenerator.CreateSchema(typeof(DuplicateSchema));
            }
            catch (Exception er)
            {
                e = er;
            }

            //this should pass whenever the change is pushed.
            Assert.IsNotNull(e);
            Assert.IsTrue("SameRoute".Contains(e.Message));
        }

        [TestMethod]
        public async void OperationLimit_WhenExceeded_Throws()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoSchema));

            var query = @"{
                  testRequest {value}
                  t2:testRequest {value}
                }";

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 1));
        }

        [TestMethod]
        public async void OperationLimit_WhenExceededWithNodes_Throws()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoSchema));

            var query = @"query{
                  t2:testRequest {value, value, value}
                }
                query{t1:testRequest{value}}
                ";

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 1));
        }

        [TestMethod]
        public async void OperationLimit_WhenExceededWithMutations_Throws()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoStateSchema));

            var query = @"
                mutation SetState{
                    setState (request:Open){
                        state
                    }
                }
                query GetState{
                    getState{
                        state
                    }
                }
            ";

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 1));
        }

        [TestMethod]
        public async void OperationLimit_WhenNotExceeded_Runs()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoSchema));

            var query = @"{
                  testRequest {value}
                  t2:testRequest {value}
                }";

            await DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 2);
        }

        [TestMethod]
        public async void OperationLimit_WhenNotExceededWithNodes_Runs()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoSchema));

            var query = @"query{
                  t2:testRequest {value, value, value}
                }
                query{t1:testRequest{value}}
                ";

            await DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 2);
        }

        [TestMethod]
        public async void OperationLimit_WhenNotExceededWithMutations_Runs()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoStateSchema));

            var query = @"
                mutation SetState{
                    setState (request:Open){
                        state
                    }
                }
                query GetState{
                    getState{
                        state
                    }
                }
            ";

            await DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 2);
        }

        [TestMethod]
        public async void OperationLimit_WhenNotExceededWithParameters_Runs()
        {
            var schemaGenerator = new SchemaGenerator(new MockServiceProvider());
            var schema = schemaGenerator.CreateSchema(typeof(EchoStateSchema));

            var query = @"
                mutation SetState($begin: Date!,  $end: Date!, $test: String, $test2: String){
                    setState (request:Open){
                        state
                    }
                }
                query GetState($begin: Date!,  $end: Date!, $test: String, $test2: String){
                    getState{
                        state
                    }
                    state2:getState{
                        state
                    }
                }
            ";

            await DocumentOperations.ExecuteOperationsAsync(schema, null, query, maxOperationNodes: 3);
        }
    }
}
