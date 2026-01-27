namespace Core.RequestModels.RicRollOut
{
    public class CreateRicRollOutRequest
    {
        public string Entitas { get; set; } = default!;
        public string JudulAplikasi { get; set; } = default!;
        public List<string>? Hashtag { get; set; }

        public List<string>? CompareWithAsIsHoldingProcessFiles { get; set; }
        public List<string>? StkAsIsToBeFiles { get; set; }

        public bool IsJoinedDomainAdPertamina { get; set; }
        public bool IsUsingErpPertamina { get; set; }
        public bool IsImplementedRequiredActivation { get; set; }
        public bool HasDataCenterConnection { get; set; }
        public bool HasRequiredResource { get; set; }

        public string? Action { get; set; }
    }
}
