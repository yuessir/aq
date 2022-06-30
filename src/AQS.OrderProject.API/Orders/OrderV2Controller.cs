using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Orders.V2.CancelOrder;
using AQS.OrderProject.Application.Orders.V2.PlaceOrder;
using AQS.OrderProject.Application.Orders.V2.UpdateHitter;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.API.Orders
{
    [Route("/v2/orders/")]
    [ApiController]
    public class OrderV2Controller : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrderV2Controller(IMediator mediator, ILogger logger)
        {
            this._mediator = mediator;
            _logger = logger;
        }

        [HttpPost("{orderId}")]
        public async Task<AqsResponse> PlaceOrder(string userName, string orderId, [FromBody] string placeOrderJson)
        {
            AqsPlaceOrderV2Req sample = new AqsPlaceOrderV2Req
            {
                CreatedTimeUtc = DateTime.UtcNow,
                HitterId = "dev",
                MarketType = AqsMarketType.AsianHandicap,
                OrderId = orderId,
                OrderMappings = "[{\"book\":\"SINGBET\",\"tournament\":\"Korea K League 1\",\"homeTeam\":\"Gyeongnam\",\"awayTeam\":\"Ulsan Hyundai\",\"scheduledKickOffTimeUtc\":\"2019-07-09T10:30:00.000Z\",\"choice\":\"Gyeongnam\",\"liveHomeScore\":0,\"liveAwayScore\":0}]".FromJson<List<OrderMapping>>(),
                Phase = AqsMarket.InRunning,
                ScoreType = "aaaa",
                Stage = AqsStage.Penalties
            };

            placeOrderJson = sample.ToJson();

            var result = await LogWrapper(nameof(PlaceOrder), userName, orderId, placeOrderJson, async () =>
              {
                  AqsPlaceOrderV2Req req = placeOrderJson.FromJson<AqsPlaceOrderV2Req>();
                  PlaceCustomerOrderCommand cmd = new(userName, req.OrderId, req.MarketType, req.OrderMappings, req.ScoreType, req.Stage, req.HitterId);

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
