using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Cards.設計試案用
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
    public class CardAssetBase : ScriptableObject
    {
        public int Id;
        public string CardName;
        [TextArea]
        public string Description;
        public int Cost;
        public int InitialLevel = 1;
    }


}