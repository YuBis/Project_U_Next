# Project_U_Next

![cs](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

코드 구조 작성에 중점을 두어 만들어진 간단한 샘플 프로젝트입니다.

2D 횡스크롤 RPG의 간단한 기능이 구현되어 있으며 간단한 [오브젝트 풀 매니저](https://github.com/YuBis/Project_U_Next/blob/main/Assets/Script/Manager/ObjectPoolManager.cs), [테이블 매니저](https://github.com/YuBis/Project_U_Next/blob/main/Assets/Script/Core/BaseManager.cs) 등이 구현되어 있습니다.

JSON으로 구성된 테이블을 사용하고 있으며, 각 캐릭터는 [팩토리 패턴](https://github.com/YuBis/Project_U_Next/blob/main/Assets/Script/Manager/AI/AIFactory.cs)으로 인젝션하는 [AI](https://github.com/YuBis/Project_U_Next/blob/main/Assets/Script/Manager/AI/BaseAI.cs)를 붙여 작동하도록 구현되어 있습니다.

GameCharacter는 [MVP 패턴](https://github.com/YuBis/Project_U_Next/tree/main/Assets/Script/Object/GameCharacter)을 이용해 구현했습니다.

에셋 스토어 및 무료 배포되는 Spine 샘플 파일을 이용하였으며, 각 AI의 상태 변경에 맞춰 Spine 애니메이션이 변경됩니다.


## Installation

Unity 2022.3.14f1 버전에서 구동됩니다.
실행 시 BaseScene 씬을 실행 후 Play하면 됩니다.
## Tech Stack

Spine unity, Pro Camera 2D, UGUI, UniTask, UniRx, ...