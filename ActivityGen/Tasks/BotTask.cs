using System.Threading.Tasks;

namespace ActivityGen.Tasks {
    public enum BotTaskResult {
        Success,
        Failed
    }

    public interface BotTask {
        Task<BotTaskResult> Do(Bot bot);
    }
}
