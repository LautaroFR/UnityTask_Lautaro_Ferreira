using UnityEngine;
using Project.Inventory;
using Project.Items;

namespace Project.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationEquipmentDriver : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private ItemDatabase itemDatabase;

        private Animator _animator;
        private RuntimeAnimatorController _defaultController;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _defaultController = _animator.runtimeAnimatorController;

            if (inventory == null)
                inventory = GetComponentInParent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (inventory != null)
                inventory.EquippedWeaponChanged += ApplyWeaponVisual;

            if (inventory != null)
                ApplyWeaponVisual(inventory.EquippedWeaponId);
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.EquippedWeaponChanged -= ApplyWeaponVisual;
        }

        private void ApplyWeaponVisual(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                _animator.runtimeAnimatorController = _defaultController;
                return;
            }

            if (itemDatabase == null || !itemDatabase.TryGet(weaponId, out var item) || item == null)
            {
                _animator.runtimeAnimatorController = _defaultController;
                return;
            }

            var overrideCtrl = item.AnimatorOverride;
            _animator.runtimeAnimatorController = overrideCtrl != null ? overrideCtrl : _defaultController;
        }
    }
}
