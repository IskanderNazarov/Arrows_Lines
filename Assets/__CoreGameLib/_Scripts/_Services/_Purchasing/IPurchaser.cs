// file: assets/_coregame/_scripts/purchasing/purchaser.cs
// assembly: corelib.asmdef

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Playgama;
using UnityEngine;

namespace Core._Purchasing {
    public interface IPurchaser {
        public event Action<string, bool> OnPurchaseCompletedEvent;

        IEnumerator Initialize(bool isSupported);
        void BuyItem(string id);
        ProductInfo GetProdInfoByID(string id);
        
        void ConsumePurchase(string id);
    }
}