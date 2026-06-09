# Spec: Hệ thống Âm thanh (Audio System) - Dự án EsLand

## 1. Mục tiêu (Objective)
Xây dựng hệ thống quản lý âm thanh (SFX, BGM, UI) chuyên nghiệp, tối ưu hiệu năng và linh hoạt. Hệ thống phải đảm bảo tách biệt hoàn toàn logic âm thanh ra khỏi logic gameplay (SOLID) và dễ dàng quản lý thông qua Unity Editor.

### Mục tiêu cụ thể:
- Phát hiệu ứng âm thanh (SFX) thông qua Object Pooling để tránh lag.
- Phát nhạc nền (BGM) hỗ trợ chuyển tiếp mượt mà.
- Quản lý âm lượng tập trung qua Unity AudioMixer.
- Cấu hình âm thanh theo hướng dữ liệu (Data-driven) sử dụng ScriptableObjects.

## 2. Tech Stack
- **Engine:** Unity (2022.3+).
- **Ngôn ngữ:** C# (SOLID, Event-Driven).
- **Dữ liệu:** ScriptableObjects.
- **Tối ưu:** Object Pooling cho AudioSource.
- **Kiểm soát:** Unity AudioMixer.

## 3. Cấu trúc Thư mục (Project Structure)
### Scripts:
- `Assets/Scripts/Core/Contracts/IAudioService.cs`: Giao diện lập trình (Interface).
- `Assets/Scripts/Data/Audio/AudioData.cs`: Định nghĩa dữ liệu âm thanh (ScriptableObject).
- `Assets/Scripts/Infrastructure/Audio/`: Chứa các class triển khai hệ thống.
    - `AudioManager.cs`: Quản lý chính (Singleton/Service).
    - `AudioPool.cs`: Hệ thống pooling cho AudioSource.
- `Assets/Scripts/Gameplay/Audio/`: Chứa các "Cầu nối" (Listeners).
    - `PlayerAudioListener.cs`, `EnemyAudioListener.cs`, etc.

### Assets:
- `Assets/Audio/SFX/`: File âm thanh hiệu ứng (.wav).
- `Assets/Audio/BGM/`: File nhạc nền (.mp3, .ogg).
- `Assets/Data/Audio/`: Các file .asset (AudioData).

## 4. Thiết kế Kỹ thuật (Technical Design)

### AudioData (ScriptableObject)
Lưu trữ thông tin cấu hình cho một âm thanh duy nhất:
- `AudioClip clip`: File âm thanh.
- `AudioMixerGroup group`: Nhóm SFX hoặc BGM.
- `float volume`: Âm lượng cơ sở.
- `float pitchRange`: Độ biến thiên cao độ (tạo sự tự nhiên).
- `bool loop`: Có lặp lại hay không.

### IAudioService (Contract)
```csharp
public interface IAudioService {
    void PlaySFX(AudioData data, Vector3 position = default);
    void PlayBGM(AudioData data, bool fade = true);
    void StopBGM(float fadeTime = 1.0f);
    void SetVolume(string parameterName, float volume);
}
```

## 5. Chiến lược Kiểm thử (Testing Strategy)
- **Manual Test:** Tạo các AudioData giả lập và kích hoạt thông qua sự kiện (ví dụ: nhấn nút UI hoặc va chạm vật lý).
- **Console Monitoring:** Kiểm tra log để đảm bảo Object Pool hoạt động đúng (lấy/trả AudioSource).
- **Inspector Check:** Đảm bảo các tham chiếu AudioMixer và AudioData không bị null.

## 6. Ranh giới (Boundaries)
- **Luôn làm:** Sử dụng `IAudioService` để phát âm thanh. Gán AudioMixerGroup cho mọi AudioData.
- **Hỏi trước:** Sử dụng các thư viện âm thanh bên ngoài (FMOD).
- **Không bao giờ:** Gọi trực tiếp `AudioSource.Play()` bên trong các class logic như `CharacterHealth` hay `EnemyBase`. Không sử dụng `AudioSource.PlayClipAtPoint`.

## 7. Tiêu chí Thành công (Success Criteria)
- Âm thanh phát ra đúng lúc khi sự kiện gameplay xảy ra.
- Không có lỗi NullReferenceException khi phát âm thanh.
- Hệ thống AudioSource Pool hoạt động ổn định, không sinh rác (GC).
- Có thể điều chỉnh âm lượng tổng thông qua AudioMixer trong Editor.
