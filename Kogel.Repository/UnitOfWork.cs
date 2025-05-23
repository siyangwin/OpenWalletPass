﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Kogel.Dapper.Extension;
using Kogel.Dapper.Extension.Helper;
using Kogel.Repository.Interfaces;

namespace Kogel.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        public IDbConnection Connection { get; }

        /// <summary>
        /// 工作单元事务
        /// </summary>
        public IDbTransaction Transaction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public UnitOfWork(IDbConnection connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="transactionMethod"></param>
        /// <param name="IsolationLevel"></param>
        /// <returns></returns>
        [UnitOfWorkAttrbute]
        public IUnitOfWork BeginTransaction(Action transactionMethod, IsolationLevel IsolationLevel = IsolationLevel.Serializable)
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();

            if (Transaction == null)
                Transaction = Connection.BeginTransaction(IsolationLevel);
            try
            {
                SqlMapper.Aop.OnExecuting += Aop_OnExecuting;
                transactionMethod.Invoke();
            }
            catch (Exception ex)
            {
                this.Rollback();
                throw ex;
            }
            finally
            {
                SqlMapper.Aop.OnExecuting -= Aop_OnExecuting;
            }
            return this;
        }


        /// <summary>
        /// 开始事务--循环使用
        /// </summary>
        /// <param name="transactionMethod"></param>
        /// <param name="IsolationLevel"></param>
        /// <returns></returns>
        [UnitOfWorkAttrbute]
        public IUnitOfWork NewTransaction(Action transactionMethod, IsolationLevel IsolationLevel = IsolationLevel.Serializable)
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();

            Transaction = Connection.BeginTransaction(IsolationLevel);

            try
            {
                SqlMapper.Aop.OnExecuting += Aop_OnExecuting;
                transactionMethod.Invoke();
            }
            catch (Exception ex)
            {
                this.Rollback();
                throw ex;
            }
            finally
            {
                SqlMapper.Aop.OnExecuting -= Aop_OnExecuting;
            }
            return this;
        }

        /// <summary>
        /// 工作单元内所有访问数据库操作执行前
        /// </summary>
        /// <param name="command"></param>
        private void Aop_OnExecuting(ref CommandDefinition command)
        {
            //相同数据库链接才会进入单元事务
            if (command.Connection.ConnectionString.Contains(this.Connection.ConnectionString))
            {
                //是否进入过工作单元(防止循环嵌套UnitOfWork)
                if (!command.IsUnifOfWork)
                {
                    command.IsUnifOfWork = true;
                    command.Connection = this.Connection;
                    command.Transaction = this.Transaction;
                }
            }
        }

        /// <summary>
        /// 提交
        /// </summary>
        public void Commit()
        {
            if (Transaction != null)
                if (!IsAnyUnitOfWork())
                    Transaction.Commit();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void Rollback()
        {
            if (Transaction != null)
                if (!IsAnyUnitOfWork())
                    Transaction.Rollback();
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        public void Dispose()
        {
            if (Transaction != null)
                Transaction.Dispose();

            if (Connection != null)
                Connection.Dispose();

            GC.SuppressFinalize(this);
        }

        ~UnitOfWork() => this.Dispose();

        /// <summary>
        /// 是否存在最外层嵌套单元
        /// </summary>
        /// <returns></returns>
        private static bool IsAnyUnitOfWork()
        {
            //嵌套的Unitofwork数量
            var count = 0;
            //当前堆栈信息
            StackTrace st = new StackTrace();
            StackFrame[] sfs = st.GetFrames();
            for (int i = 1; i < sfs.Length; ++i)
            {
                //非用户代码,系统方法及后面的都是系统调用，不获取用户代码调用结束
                if (StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset()) break;
                var method = sfs[i].GetMethod();//方法
                if (method.CustomAttributes.Any(x => x.AttributeType == typeof(UnitOfWorkAttrbute)))
                    count++;
            }
            return count > 0;
        }

        /// <summary>
        /// 仓储方法标记 （内部使用）
        /// </summary>
        private sealed class UnitOfWorkAttrbute : Attribute
        {
        }
    }
}
