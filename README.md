# Pre-Post Analysis Tool (C# Portable Version)

## 📌 소개 (Introduction)
이 프로그램은 사회복지 실천 현장 및 연구에서 사전-사후 검사 결과를 손쉽게 분석할 수 있도록 돕는 도구입니다. 복잡한 통계 프로그램 없이, 데이터를 복사해 붙여넣는 것만으로 결과를 즉시 확인할 수 있습니다.

**주요 특징:**
- **No Setup Required**: C# WinForms 기반 단일 실행 파일(.exe)로, 별도 설치 없이 바로 실행 가능합니다.
- **Bilingual Interface**: 한국어와 영어를 지원하며, 시스템에 따라 초기 언어가 설정되거나 메뉴에서 언어를 변경할 수 있습니다.
- **Auto-Detection**: 표본 수(N)와 정규성(Normality) 여부를 자동으로 판단하여 최적의 검정 방법(t-test or Wilcoxon Test)을 선택 및 제안합니다.
- **Report Generation**: 분석 결과를 논문이나 보고서에 바로 사용할 수 있도록 APA 스타일의 문장으로 자동 생성해줍니다.

## 🛠️ 개발자 노트 (Developer Notes)

### 아키텍처 및 구현 방식
- **Framework**: .NET Framework 4.0 이상 (Windows 기본 내장) 호환.
- **Language**: C# (WinForms)
- **Design Pattern**: 단일 파일 배포를 위해 리소스나 외부 DLL 의존성을 최소화하여 개발되었습니다. HTML 리포트 생성 로직을 내장하여 `WebBrowser` 컨트롤을 통해 풍부한 서식의 결과를 보여줍니다.

### 통계 로직
1. **기술 통계 (Descriptive)**: 평균, 표준편차 계산.
2. **정규성 검정 (Normality Test)**: Jarque-Bera 검정을 사용하여 데이터가 정규분포를 따르는지 판단합니다.
3. **가설 검정 (Hypothesis Test)**:
   - **N < 10**: 비모수 검정인 윌콕슨 부호순위 검정(Wilcoxon Signed-Rank Test) 자동 적용.
   - **10 ≤ N < 15**: 정규성 여부에 따라 t-test 또는 Wilcoxon을 선택하되, 두 결과 모두 참고하도록 안내.
   - **N ≥ 15**: 정규성 검정 결과에 따라 적절한 분석 방법 자동 선택.

### 빌드 방법 (How to Build)
이 소스 코드는 `Build_universal.bat` 스크립트를 통해 컴파일할 수 있습니다.
시스템에 .NET Framework(csc.exe)가 설치되어 있어야 합니다.

```cmd
build_universal.bat
```

빌드가 성공하면 `Pre-Post-Analysis.exe` 파일이 생성됩니다.

---
Produced and distributed by **welfareact.net**
