using System;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Orders.V2.CancelOrder;
using AQS.OrderProject.Application.Orders.V2.UpdateHitter;
using AQS.OrderProject.Application.Orders.V3.PlaceOrder;
using AQS.OrderProject.Application.Orders.V3.PrepareOrder;
using AQS.OrderProject.Application.Orders.V3.UpdateOrder;
using AQS.OrderProject.Domain.Reponses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.API.Orders
{
    [Route("/v3/orders/")]
    [ApiController]
    public class OrderV3Controller : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrderV3Controller(IMediator mediator, ILogger logger)
        {
            this._mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// AQS V3 - 此欄位 UK 沒給, 給系統紀錄用; 問看看 UK 可不可以塞;
        /// UK 回答:
        ///     The prepare order request is a special case i.e we would not be using that most of the time.
        ///     We'll just call place order just as we are doing at the moment.
        ///     The prepare order request does not really signify the creation of an order that's why the createTimeUtc isn't part of the content.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="orderId"></param>
        /// <param name="prepareOrderJson"></param>
        /// <returns></returns>
        [HttpPost("prepare/{orderId}")]
        public async Task<AqsResponse> PrepareOrder(string currentUser, string orderId, [FromBody] string prepareOrderJson)
        {
            var result = await LogWrapper(nameof(PrepareOrder), currentUser, orderId, prepareOrderJson, async () =>
            {
                AqsPrepareOrderReq req = prepareOrderJson.FromJson<AqsPrepareOrderReq>();
                req.ReceivedReqTime = DateTime.UtcNow;
                PrepareOrderCommand cmd = new(req.OrderId.ToGuid(), req.HitterId, req.OrderMappings, req.Translation, req.ReceivedReqTime);

                AqsResponse aqsResp = await _mediator.Send(cmd);

                return aqsResp;
            });

            return result;
        }


        [HttpPost("{orderId}")]
        public async Task<AqsResponse> PlaceOrder(string userName, string orderId, [FromBody] string placeOrderJson)
        {
            var result = await LogWrapper(nameof(PlaceOrder), userName, orderId, placeOrderJson, async () =>
              {
                  AqsPlaceOrderV3Req req = placeOrderJson.FromJson<AqsPlaceOrderV3Req>();
                  PlaceOrderCommand cmd = new(req.OrderId.ToGuid(),
                      req.Phase,
                      req.ScoreType,
                      req.Stage,
                      req.MarketType,
                      req.HitterId,
                      req.Stake,
                      req.OrderMappings,
                      req.Translation,
                      req.OrderLines);

                  AqsResponse aqsResp = await _mediator.Send(cmd);

                  return aqsResp;
              });

            return result;
        }

        [HttpPost("{orderId}/updatePrice")]
        public async Task<AqsResponse> UpdateOrder(string currentUser, string orderId, [FromBody] string updateOrderJson)
        {
            var result = await LogWrapper(nameof(UpdateOrder), currentUser, orderId, updateOrderJson, async () =>
            {
                AqsUpdateOrderReq req = updateOrderJson.FromJson<AqsUpdateOrderReq>();
                UpdateOrderCommand cmd = new(req.OrderId.ToGuid(), req.CreatedTime, req.HitterId, req.Stake, req.OrderLines);

                AqsResponse aqsResp = await _mediator.Send(cmd);

                return aqsResp;
            });

            return result;
        }

        [HttpPost("{orderId}/updateHitter")]
        public async Task<AqsResponse> UpdateHitter(string userName, string orderId, [FromBody] string updateHitterJson)
        {
            var result = await LogWrapper(nameof(UpdateHitter), userName, orderId, updateHitterJson, async () =>
            {
                AqsUpdateHitterReq req = updateHitterJson.FromJson<AqsUpdateHitterReq>();
                UpdateHitterCommand cmd = new(req.OrderId.ToGuid(), req.CreatedTimeUtc, req.HitterId);

                AqsResponse aqsResp = await _mediator.Send(cmd);

                return aqsResp;
            });

            return result;
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<AqsResponse> CancelOrder(string userName, string orderId, [FromBody] string cancelOrderJson)
        {
            var result = await LogWrapper(nameof(CancelOrder), userName, orderId, cancelOrderJson, async () =>
            {
                AqsUpdateHitterReq req = cancelOrderJson.FromJson<AqsUpdateHitterReq>();
                CancelOrderCommand cmd = new(req.OrderId.ToGuid(), req.CreatedTimeUtc, req.HitterId);

                AqsResponse aqsResp = await _mediator.Send(cmd);

                return aqsResp;
            });

            return result;
        }

        private async Task<AqsResponse> LogWrapper(string actionName, string userName, string orderId, string cancelOrderJson, Func<Task<AqsResponse>> func)
        {
            _logger.Information("[{}] Received request from user: <{}>, content: {}", actionName, userName, cancelOrderJson);

            long startTime = DateTime.UtcNow.UnixTimestamp();

            var aqsResp = await func();

            long spentTime = DateTime.UtcNow.UnixTimestamp() - startTime;

            _logger.Information("[{}] OrderId: <{}>, spent-time: {} ms", actionName, orderId, spentTime);

            return aqsResp;
        }
    }
}
