using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.Loader;
using Common;
using Common.Types;

namespace ClownAIO {
    internal static class BotContext {
        public static readonly ObservableConcurrentDictionary<BotType, Type> BotTypes = new ObservableConcurrentDictionary<BotType, Type>();
        public static readonly ObservableCollection<Bot> Bots = new ObservableCollection<Bot>();
        public static readonly ObservableConcurrentDictionary<string, BillingProfile> Profiles = new ObservableConcurrentDictionary<string, BillingProfile>();
    }
}
