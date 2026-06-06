using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Performance
{
    /// <summary>
    /// Quản lý việc culling hàng loạt vật thể để tối ưu FPS.
    /// Sử dụng kỹ thuật Staggered Update để tránh gây khựng hình.
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [Header("Culling Settings")]
        [SerializeField] private float cullingRadius = 30f;
        [SerializeField] private int objectsPerFrame = 500;
        [SerializeField] private float checkInterval = 0.5f;

        private List<CullableEntity> _entities = new List<CullableEntity>();
        private Transform _playerTransform;
        private float _cullingRadiusSqr;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _cullingRadiusSqr = cullingRadius * cullingRadius;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(CullingRoutine());
        }

        public void Register(CullableEntity entity)
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }

        public void Unregister(CullableEntity entity)
        {
            _entities.Remove(entity);
        }

        private IEnumerator CullingRoutine()
        {
            while (true)
            {
                // Tìm Player nếu chưa có
                if (_playerTransform == null)
                {
                    var player = GameObject.FindObjectOfType<Gameplay.Characters.Player>();
                    if (player != null) _playerTransform = player.transform;
                }

                if (_playerTransform != null)
                {
                    Vector3 playerPos = _playerTransform.position;

                    // Xử lý chia nhỏ danh sách qua nhiều frame
                    // Không dùng biến 'total' cố định vì danh sách có thể thay đổi (Add/Remove) khi đang yield
                    for (int i = 0; i < _entities.Count; i++)
                    {
                        var entity = _entities[i];
                        if (entity == null) 
                        {
                            _entities.RemoveAt(i);
                            i--; // Lùi chỉ số để không bỏ sót phần tử tiếp theo sau khi xóa
                            continue;
                        }

                        float distSqr = (entity.transform.position - playerPos).sqrMagnitude;
                        entity.SetCullState(distSqr > _cullingRadiusSqr);

                        // Nếu đã xử lý đủ số lượng cho frame này, đợi frame tiếp theo
                        if (i > 0 && i % objectsPerFrame == 0)
                        {
                            yield return null;
                        }
                    }
                }

                yield return new WaitForSeconds(checkInterval);
            }
        }

        // Visualize bán kính culling trong Editor
        private void OnDrawGizmosSelected()
        {
            if (_playerTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_playerTransform.position, cullingRadius);
            }
        }
    }
}
