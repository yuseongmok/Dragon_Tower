# Eric VFX Studio 렌더 파이프라인 메모

이 DragonTower 프로젝트는 Built-in Render Pipeline을 사용합니다.

VFX 프리팹의 실행에 필요한 Unity 기본 모듈 `com.unity.modules.particlesystem` 1.0.0을 `Packages/manifest.json`에 등록했습니다. 이 항목이 빠지면 `UnityEngine.ParticleSystemModule`을 찾을 수 없다는 CS1069 오류가 발생합니다.

에셋에 함께 포함된 URP 전용 `AdditiveFlow.shader`와 `AlphaBlendFlow.shader`의 원본은 URP 패키지를 요구하므로 `Resource/Shader/URP~`에 보존했습니다. Unity는 이름이 `~`로 끝나는 폴더를 가져오지 않습니다. 원래 경로에는 기존 머티리얼 연결 ID와 셰이더 이름을 유지하는 Built-in 호환판을 두어 분홍색 머티리얼과 URP include 오류를 함께 방지했습니다.

게임에서는 `Game VFX - Cartoon Skill Effects/Prefabs/Built-In`의 프리팹을 사용하세요. 나중에 프로젝트 전체를 URP로 전환할 때만 `URP~`의 두 원본 셰이더를 현재 호환판과 교체하면 됩니다.

## 현재 스킬 연결

| 드래곤·스킬 | 적용한 Built-in 프리팹 | 동시 파티클 시스템 제한 |
|---|---|---:|
| 엠버 · 화염 폭발 | FX_Fire_Exp | 7 |
| 루나 · 서리 숨결 | FX_Blue Stab | 10 |
| 제피르 · 질풍 강타 | FX_Impact Airflow | 13 |
| 블랜디 · 모래 돌풍 | FX_Ground Shockwave | 18 |
| 볼트 · 전기 쇼크 | FX_Cartoon Thunder | 11 |
| 옥타 · 버블 샷 | FX_Splash_Hit | 9 |
| 노바 · 어둠 일격 | FX_PinkMagicArrow_Hit | 9 |
| 단테 · 섬광 폭발 | FX_FlashShot_Orange | 13 |

전투 UI는 Screen Space Overlay이므로 일반 파티클을 바로 자식으로 넣으면 화면 뒤에 가려집니다. `BattleAssetVfx`가 480×850 투명 렌더 텍스처에 선택된 프리팹을 그리고 기존 UI 효과 레이어에 합성합니다. 선택된 드래곤의 효과 한 개만 미리 만들고 스킬을 쓸 때 재사용하므로 매번 생성·삭제하지 않습니다. 효과가 끝나면 전용 카메라를 끄고 렌더 텍스처를 비웁니다.

에셋 원본의 Additive 셰이더는 최종 알파를 항상 1로 출력해 렌더 텍스처에서 투명한 파티클 사각형이 검은 도형으로 보였습니다. RGB와 실제 파티클 커버리지를 함께 저장하도록 수정하고, 합성 단계에는 premultiplied alpha 전용 `VfxComposite.shader`를 사용합니다. 지원되지 않는 머티리얼은 프리팹 원본을 변경하지 않고 실행 중 Built-in Additive 셰이더 사본으로 교체해 분홍색 사각형을 방지합니다.

각 `SkillData`의 **Battle VFX**, **Vfx Scale**, **Vfx Offset**, **Vfx Duration**, **Vfx System Limit**을 바꾸면 코드를 수정하지 않고 프리팹·크기·위치·지속시간·성능 제한을 조절할 수 있습니다. 현재는 개별 파티클 시스템의 최대 파티클 수도 64개로 제한하고 에셋의 Light 컴포넌트를 끕니다.
