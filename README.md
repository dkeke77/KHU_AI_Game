# KHU Lecture "AI & Game Progamming" Term Project

2025년 1학기 "인공지능과게임프로그래밍" 과목 텀프로젝트 깃허브 페이지입니다.




---

## 캐릭터 생성 권장 방법

프리팹 **Sword_Man_Origin**은 캐릭터의 원본 프리팹입니다.

AI를 추가할 때, 해당 프리팹을 직접 수정하기보다, **프리팹 Variant**를 활용하길 권장합니다.

<프리팹 Variant 이미지>

프리팹 Variant는 **Sword_Man_Origin** 우클릭 -> Create 선택 -> Prefab Variant 선택

위와 같이 하여 프리팹 Variant를 생성할 수 있으며, 그렇게 생성된 프리팹 Variant에서 기능을 구현하길 권장합니다.






## core 스크립트 함수 사용법

캐릭터의 모든 기능은 **core** 스크립트에 구현되어있으며 해당 스크립트를 통해 동작을 호출할 수 있습니다.

또한 모든 동작에는 "해당 동작을 할 수 있는 상태인지 확인하는 함수"가 있습니다.

캐릭터에 "구현된 동작"과 "그 동작이 가능한지 확인하는 함수"는 아래와 같습니다.

|Action|isAble?|
|:---:|:---:|
|HandleMovement(x,z)|CanMove()|
|Attack()|CanAttack()|
|Defence()|CanDefence()|
|Dodge()|CanDodge()|

<작성 예시 이미지>

동작을 호출하기 전에는 위와 동작 가능여부를 확인하길 권장합니다.




## CharacterInfo

AI 작성 시, 캐릭터에 대한 모든 정보를 쉽게 호출할 수 있게끔 CharacterInfo 스크립트를 작성했습니다.

캐릭터에 대한 위치, 상태 등을 얻을 수 있으며 호출가능한 변수와 그 설명은 아래와 같습니다.

|이름|설명|
|:---:|:---|
|CurrentHP|캐릭터의 현재 HP|
|IsDead|캐릭터의 사망 여부|
|IsCollideWithCharacter|캐릭터간 충돌 발생 여부|
|Position|캐릭터의 현재 월드 좌표|
|Forward|캐릭터가 현재 향하고 있는 방향|
|speed|캐릭터의 속력|
|CurrentState|캐릭터의 현재 State|
|IsDodging|캐릭터가 현재 회피중인지 여부|
|IsBlocking|캐릭터가 현재 방어중인지 여부|
|CanAttack|캐릭터가 현재 공격이 가능한지 여부|
|CanDefend|캐릭터가 현재 방어가 가능한지 여부|
|CanDodge|캐릭터가 현재 회피가 가능한지 여부|

그리고 캐릭터의 조인트별 위치를 얻을 수 있는 **GetJointPosition** 함수가 있습니다.

해당 함수의 파라미터에 조인트의 이름을 문자열로 넣으면 그에 맞는 조인트의 월드 좌표를 반환합니다.

확인 가능한 조인트는 아래와 같습니다.

  * Head
  * RightHand
  * LeftHand
  * RightArm
  * LeftArm
  * RightFoot
  * LeftFoot
  * Spine

