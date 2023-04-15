using BreakMeGrpcService;
using Grpc.Core;
using BreakMe;
using BreakMeGrpcService.DataObj;
using BreakMeGrpcService.Local;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace BreakMeGrpcService.Services
{
    public class BreakMeService : BreakMe.BreakMe.BreakMeBase
    {
        private readonly ILogger<BreakMeService> _logger;
        public BreakMeService(ILogger<BreakMeService> logger)
        {
            _logger = logger;
        }

        public override async Task<CreateResp> CreateIntrupt(IntruptInfo request, ServerCallContext context)
        {
            IntpData data = new IntpData(
                Guid.NewGuid(),
                request.IntpTime,
                request.HasIntpProgressPath ? request.IntpProgressPath : null,
                request.IntpTy,
                request.HasObserveMode ? request.ObserveMode : ObserveMode.Executable,
                request.HasIntpMessage ? request.IntpMessage : "Breaking!");

            var uid = await FileManager.CreateIntp(data);

            if (uid == null)
            {
                return new CreateResp { IsSuccess = false, IntpId = "" };

            }
            else
            {
                return new CreateResp { IsSuccess = true, IntpId = uid.ToString() };
            }
        }

        public override async Task<Config> FetchConfig(Empty request, ServerCallContext context)
        {
            var cfg = await FileManager.GetConfig();
            var ret_cfg = new Config { LeaveTimeBound = cfg.LeaveTimeBound, MuitlTaskNum = cfg.MuiltTaskNum };

            ret_cfg.WhiteList.AddRange(cfg.WhiteList);

            return ret_cfg;
        }

        public override async Task<IntpList> FetchAllIntrupt(Empty request, ServerCallContext context)
        {
            var ret_list = new IntpList();
            foreach (var item in await FileManager.GetAllIntpTask())
            {
                var tmp = new IntruptInfo {

                    IntpProgressPath = item.IntpMessage
                    , IntpTime = item.IntpTime,
                    IntpTy = item.InterruptType,
                    ObserveMode = item.ObserveMode,
                    IntpMessage = item.IntpMessage,
                };
                ret_list.AllInfo.Add(item.Id.ToString(), tmp);
            }
            return ret_list;
        }

        public override async Task<SetConfigResp> UpdateConfig(SetConfigReq request, ServerCallContext context)
        {
            var old = await FileManager.GetConfig();

            if (request.HasLeaveTimeBound)
            {
                old.LeaveTimeBound = request.LeaveTimeBound;
            }
            if (request.HasMuitlTaskNum)
            {
                old.MuiltTaskNum = request.MuitlTaskNum;
            }

            var wihtelist = request.WhiteList;
            if (wihtelist.Mode == WhiteListUpdateMode.Overwrite)
            {
                old.WhiteList = wihtelist.PrgressPathList;
            }
            else
            {

                foreach (var item in wihtelist.PrgressPathList)
                {
                    old.WhiteList.Add(item);
                }

            }

            FileManager.updateConfig(old);

            return new SetConfigResp { IsSuccess=true };
        }
    }


}