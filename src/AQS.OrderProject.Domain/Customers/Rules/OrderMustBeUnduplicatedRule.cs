using System;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SeedWork;
using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Customers.Rules;

public class OrderMustBeUnduplicatedRule : IBusinessRule
{
    private readonly IOrderDuplicateChecker _orderDuplicateChecker;
    private string _orderId;
    private string _userName;

    public OrderMustBeUnduplicatedRule(string orderId,
        IOrderDuplicateChecker orderDuplicateChecker,
        string userName)
    {
        _orderDuplicateChecker = orderDuplicateChecker;
        this._orderId = orderId;
        this._userName = userName;
    }

    public bool IsBroken() => !_orderDuplicateChecker.IsDuplicated(_orderId, this._userName);

    public string Message => $"Duplicated orderId: {_orderId}";
}