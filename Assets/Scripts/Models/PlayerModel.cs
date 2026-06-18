using System;

namespace Cinderkeep.Gameplay
{
    // 플레이 중 저장되어야 하는 플레이어 Instance Data입니다.
    // 변하지 않는 기획 데이터는 GameData, 변하는 저장 데이터는 Model 이름을 붙입니다.
    [Serializable]
    public sealed class PlayerModel
    {
        public const string ResourceWood = "Wood";
        public const string ResourceStone = "Stone";
        public const string ResourceIron = "Iron";
        public const string ResourceGold = "Gold";
        public const string ResourceMithril = "Mithril";
        public const string ResourceAdamantium = "Adamantium";

        private int _health;
        private int _maxHealth;
        private int _stamina;
        private int _maxStamina;
        private int _level;
        private int _wood;
        private int _stone;
        private int _iron;
        private int _gold;
        private int _mithril;
        private int _adamantium;

        public int Health
        {
            get
            {
                return _health;
            }
        }

        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
        }

        public int Stamina
        {
            get
            {
                return _stamina;
            }
        }

        public int MaxStamina
        {
            get
            {
                return _maxStamina;
            }
        }

        public int Level
        {
            get
            {
                return _level;
            }
        }

        public int Wood
        {
            get
            {
                return _wood;
            }
        }

        public int Stone
        {
            get
            {
                return _stone;
            }
        }

        public int Iron
        {
            get
            {
                return _iron;
            }
        }

        public int Gold
        {
            get
            {
                return _gold;
            }
        }

        public int Mithril
        {
            get
            {
                return _mithril;
            }
        }

        public int Adamantium
        {
            get
            {
                return _adamantium;
            }
        }

        public event Action OnResourceChanged;

        public void InitializeDefault()
        {
            _maxHealth = 100;
            _health = _maxHealth;
            _maxStamina = 150;
            _stamina = _maxStamina;
            _level = 1;

            _wood = 0;
            _stone = 0;
            _iron = 0;
            _gold = 0;
            _mithril = 0;
            _adamantium = 0;
        }

        public void AddResource(string resourceType, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (TryAddResource(resourceType, amount))
            {
                NotifyResourceChanged();
            }
        }

        public bool UseResource(string resourceType, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            bool isSuccess = TryUseResource(resourceType, amount);
            if (isSuccess)
            {
                NotifyResourceChanged();
            }

            return isSuccess;
        }

        private bool TryAddResource(string resourceType, int amount)
        {
            string safeResourceType = NormalizeResourceType(resourceType);

            switch (safeResourceType)
            {
                case ResourceWood:
                    _wood += amount;
                    return true;
                case ResourceStone:
                    _stone += amount;
                    return true;
                case ResourceIron:
                    _iron += amount;
                    return true;
                case ResourceGold:
                    _gold += amount;
                    return true;
                case ResourceMithril:
                    _mithril += amount;
                    return true;
                case ResourceAdamantium:
                    _adamantium += amount;
                    return true;
            }

            return false;
        }

        private bool TryUseResource(string resourceType, int amount)
        {
            string safeResourceType = NormalizeResourceType(resourceType);

            switch (safeResourceType)
            {
                case ResourceWood:
                    return TrySpendWood(amount);
                case ResourceStone:
                    return TrySpendStone(amount);
                case ResourceIron:
                    return TrySpendIron(amount);
                case ResourceGold:
                    return TrySpendGold(amount);
                case ResourceMithril:
                    return TrySpendMithril(amount);
                case ResourceAdamantium:
                    return TrySpendAdamantium(amount);
            }

            return false;
        }

        private bool TrySpendWood(int amount)
        {
            if (_wood < amount)
            {
                return false;
            }

            _wood -= amount;
            return true;
        }

        private bool TrySpendStone(int amount)
        {
            if (_stone < amount)
            {
                return false;
            }

            _stone -= amount;
            return true;
        }

        private bool TrySpendIron(int amount)
        {
            if (_iron < amount)
            {
                return false;
            }

            _iron -= amount;
            return true;
        }

        private bool TrySpendGold(int amount)
        {
            if (_gold < amount)
            {
                return false;
            }

            _gold -= amount;
            return true;
        }

        private bool TrySpendMithril(int amount)
        {
            if (_mithril < amount)
            {
                return false;
            }

            _mithril -= amount;
            return true;
        }

        private bool TrySpendAdamantium(int amount)
        {
            if (_adamantium < amount)
            {
                return false;
            }

            _adamantium -= amount;
            return true;
        }

        private void NotifyResourceChanged()
        {
            if (OnResourceChanged == null)
            {
                return;
            }

            OnResourceChanged.Invoke();
        }

        private string NormalizeResourceType(string resourceType)
        {
            if (string.IsNullOrEmpty(resourceType))
            {
                return string.Empty;
            }

            string loweredType = resourceType.ToLowerInvariant();
            switch (loweredType)
            {
                case "wood":
                    return ResourceWood;
                case "stone":
                    return ResourceStone;
                case "iron":
                    return ResourceIron;
                case "gold":
                    return ResourceGold;
                case "mithril":
                    return ResourceMithril;
                case "adamantium":
                    return ResourceAdamantium;
            }

            return resourceType;
        }
    }
}
