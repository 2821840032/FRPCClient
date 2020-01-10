using FRPCClient.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRPCClient.AttributeEntity
{
    /// <summary>
    /// 请求过滤器
    /// </summary>
    public abstract class  RequestFilterAttribte:Attribute
    {
        /// <summary>
        /// 标签  仅仅用来做注释使用
        /// </summary>
        public string Label { get; set; }


        /// <summary>
        /// 请求前
        /// </summary>
        /// <param name="baseProvideServices">实例化对象</param>
        /// <param name="result">如果不为null则跳过 null执行实际内容</param>
        /// <param name="returnType">返回值结果的Type</param>
        /// <returns>false 则立即返回 不会发送任何数据</returns>
        public abstract bool BeforeExecution(BaseProvideServices baseProvideServices,ref object result,Type returnType);

        /// <summary>
        /// 请求后
        /// </summary>
        /// <param name="baseProvideServices">实例化对象</param>
        /// <param name="result">实例化运算的值</param>
        /// <param name="returnType">返回值结果的Type</param>
        /// <returns>false 则立即返回 不会发送任何数据</returns>
        public abstract bool Afterxecution(BaseProvideServices baseProvideServices,ref object result, Type returnType);
    }
}
