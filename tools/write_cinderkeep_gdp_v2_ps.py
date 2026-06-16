from pathlib import Path

ps = r'''
$ErrorActionPreference = "Stop"

$out = "C:\Users\user\Desktop\Downloads\Cinderkeep_GDP_V2.pptx"
$asset = "C:\Users\user\Desktop\Downloads\CK 게임 컨셉"
$more = Join-Path $asset "More"
$work = "C:\Users\user\AppData\Local\Temp\cinderkeep-gdp-v2"
$preview = Join-Path $work "preview"
New-Item -ItemType Directory -Force -Path $preview | Out-Null

$W = 1280
$H = 720
$C = @{
  bg = "#07111D"; panel = "#0D1B2A"; panel2 = "#102338"; ink = "#F4F7FB"; mute = "#B8C6D8";
  dim = "#7F94AA"; line = "#31516D"; ember = "#FF9D2E"; flame = "#FF5A1E";
  cyan = "#64C7E8"; green = "#35D39A"; red = "#F0525C"; white = "#FFFFFF"; black = "#000000"
}
$Font = "맑은 고딕"

function Rgb($hex) {
  $h = $hex.TrimStart("#")
  $r = [Convert]::ToInt32($h.Substring(0,2),16)
  $g = [Convert]::ToInt32($h.Substring(2,2),16)
  $b = [Convert]::ToInt32($h.Substring(4,2),16)
  return $r + ($g * 256) + ($b * 65536)
}

function Set-NoProof($range) {
  try { $range.LanguageID = 1024 } catch {}
}

function Set-Text($shape, $text, $size, $color, $bold, $align) {
  $shape.TextFrame.TextRange.Text = $text
  $tr = $shape.TextFrame.TextRange
  $tr.Font.Name = $Font
  $tr.Font.Size = $size
  $tr.Font.Color.RGB = Rgb $color
  $tr.Font.Bold = $(if ($bold) { -1 } else { 0 })
  $tr.ParagraphFormat.Alignment = $align
  Set-NoProof $tr
  try {
    $shape.TextFrame2.TextRange.Font.Name = $Font
    $shape.TextFrame2.TextRange.LanguageID = 1024
  } catch {}
  $shape.TextFrame.MarginLeft = 8
  $shape.TextFrame.MarginRight = 8
  $shape.TextFrame.MarginTop = 4
  $shape.TextFrame.MarginBottom = 4
  $shape.TextFrame.WordWrap = -1
}

function Add-Text($slide, $text, $x, $y, $w, $h, $size, $color=$C.ink, $bold=$false, $align=1) {
  $shape = $slide.Shapes.AddTextbox(1, $x, $y, $w, $h)
  Set-Text $shape $text $size $color $bold $align
  return $shape
}

function Add-Rect($slide, $x, $y, $w, $h, $fill, $trans=0.0, $line="none", $round=$true) {
  $type = $(if ($round) { 5 } else { 1 })
  $shape = $slide.Shapes.AddShape($type, $x, $y, $w, $h)
  $shape.Fill.ForeColor.RGB = Rgb $fill
  $shape.Fill.Transparency = [single]$trans
  if ($line -eq "none") {
    $shape.Line.Visible = 0
  } else {
    $shape.Line.Visible = -1
    $shape.Line.ForeColor.RGB = Rgb $line
    $shape.Line.Weight = 1.5
  }
  return $shape
}

function Add-Panel($slide, $x, $y, $w, $h, $trans=0.08, $lineColor=$C.line) {
  return Add-Rect $slide $x $y $w $h $C.panel $trans $lineColor $true
}

function Add-BoxText($slide, $text, $x, $y, $w, $h, $lineColor, $fillColor=$C.panel, $size=22, $bold=$true, $align=2) {
  $shape = Add-Rect $slide $x $y $w $h $fillColor 0.0 $lineColor $true
  Set-Text $shape $text $size $C.ink $bold $align
  return $shape
}

function Add-Line($slide, $x1, $y1, $x2, $y2, $color=$C.line, $weight=2, $arrow=$false) {
  $ln = $slide.Shapes.AddLine($x1, $y1, $x2, $y2)
  $ln.Line.ForeColor.RGB = Rgb $color
  $ln.Line.Weight = $weight
  if ($arrow) { $ln.Line.EndArrowheadStyle = 3 }
  return $ln
}

function Add-Image($slide, $path, $x, $y, $w, $h) {
  return $slide.Shapes.AddPicture($path, $false, $true, $x, $y, $w, $h)
}

function Add-Header($slide, $kicker, $title, $subtitle) {
  Add-Text $slide $kicker 72 42 520 28 14 $C.ember $true 1 | Out-Null
  Add-Text $slide $title 72 86 1040 58 38 $C.ink $true 1 | Out-Null
  if ($subtitle.Length -gt 0) { Add-Text $slide $subtitle 74 154 1050 40 18 $C.mute $false 1 | Out-Null }
}

function Add-Footer($slide, $n) {
  Add-Text $slide ("Cinderkeep GDP V2  |  {0:D2}" -f $n) 72 686 260 20 9 $C.dim $false 1 | Out-Null
}

function New-Slide($pres, $n) {
  $slide = $pres.Slides.Add($n, 12)
  $slide.FollowMasterBackground = 0
  $bg = $slide.Shapes.AddShape(1, 0, 0, $W, $H)
  $bg.Fill.ForeColor.RGB = Rgb $C.bg
  $bg.Line.Visible = 0
  $bg.ZOrder(1)
  return $slide
}

function Img($name) { return Join-Path $asset $name }
function MoreImg($name) { return Join-Path $more $name }

$pp = $null
$pres = $null
try {
  if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Force }
  $pp = New-Object -ComObject PowerPoint.Application
  $pp.Visible = -1
  $pp.DisplayAlerts = 1
  $pres = $pp.Presentations.Add()
  $pres.PageSetup.SlideWidth = $W
  $pres.PageSetup.SlideHeight = $H

  # 1
  $s = New-Slide $pres 1
  Add-Image $s (MoreImg "ck-more-01-flame-heart-close.png") 0 0 $W $H | Out-Null
  Add-Rect $s 0 0 $W $H $C.black 0.42 "none" $false | Out-Null
  Add-Panel $s 62 64 590 520 0.05 $C.line | Out-Null
  Add-Text $s "OVERVIEW (개요)" 94 94 350 28 15 $C.ember $true 1 | Out-Null
  Add-Text $s "Cinderkeep" 92 154 520 78 60 $C.ink $true 1 | Out-Null
  Add-Text $s "잿불 성채: 마지막 불씨" 96 240 520 42 32 $C.ink $true 1 | Out-Null
  Add-Text $s "1인칭 생존 로그라이트 + 베이스 방어" 98 326 500 34 24 $C.cyan $true 1 | Out-Null
  Add-Text $s "개발 기간: 2026.06.16 - 2026.07.03`nMVP 목표: 3일 생존, 15분 플레이 루프`n승리 조건: 3일차 밤 최종 보스 처치" 98 384 500 106 21 $C.ink $false 1 | Out-Null
  Add-Text $s "팀: 천우영, 정동원, 지재욱, 김성광, 최재호, 김동혁, 권성혁, 강희원, 김민석" 98 528 500 36 15 $C.mute $false 1 | Out-Null
  Add-Footer $s 1

  # 2
  $s = New-Slide $pres 2
  Add-Header $s "GAME SUMMARY (게임 요약)" "낮에는 준비하고, 밤에는 불꽃 심장을 지킨다" "플레이어가 해야 할 일은 세 문장으로 설명되어야 한다."
  Add-Panel $s 90 238 1100 290 0.02 $C.line | Out-Null
  Add-Text $s "1. 낮 3분 동안 자원, 음식, 코인, 상자를 챙긴다." 128 282 1020 42 28 $C.ink $true 1 | Out-Null
  Add-Text $s "2. 밤 2분 동안 몰려오는 적에게서 불꽃 심장을 지킨다." 128 366 1020 42 28 $C.ink $true 1 | Out-Null
  Add-Text $s "3. 3일차 밤에는 보스가 3분 동안 접근하고, 보스를 잡으면 클리어한다." 128 450 1020 42 28 $C.ink $true 1 | Out-Null
  Add-Text $s "게임의 목적은 '더 오래 버티기'가 아니라, 짧은 시간 안에 성장과 방어 선택을 끝내는 것이다." 140 592 1000 34 21 $C.ember $true 2 | Out-Null
  Add-Footer $s 2

  # 3
  $s = New-Slide $pres 3
  Add-Image $s (MoreImg "ck-more-02-day-gathering.png") 0 0 $W $H | Out-Null
  Add-Rect $s 0 0 $W $H $C.black 0.34 "none" $false | Out-Null
  Add-Panel $s 70 76 1140 500 0.02 $C.line | Out-Null
  Add-Header $s "FIRST BUILD GUIDE (빠른 1차 개발 가이드)" "MVP는 3일, 약 15분짜리 한 판이다" "첫 버전은 기능 욕심보다 3일 생존 루프가 실제로 도는 것을 우선한다."
  Add-Text $s "이번 버전에 반드시 넣는다" 112 246 430 34 26 $C.green $true 1 | Out-Null
  Add-Text $s "나중에 확장한다" 720 246 360 34 26 $C.dim $true 1 | Out-Null
  Add-Text $s "• 1인칭 이동 / 시점 / 공격`n• 낮 3분, 밤 2분, 아침 보상 15초`n• 자원 채집, 장비 제작, 상자 유물`n• 불꽃 심장 체력 / 온기 / 레이저`n• 고정 건축 지점: 벽, 포탑, 함정`n• 3일차 밤 보스 접근 3분 + 처치 클리어" 116 296 520 220 22 $C.ink $false 1 | Out-Null
  Add-Text $s "• 자유 건축과 건물 이동/회전`n• 7일차 이상 장기 루프`n• 희귀 장비와 세트 옵션`n• 추위 게이지와 방한 장비`n• 화살 제작 / 탄약 관리`n• 자동 장착과 세부 슬롯 효과" 724 296 420 220 22 $C.mute $false 1 | Out-Null
  Add-Footer $s 3

  # 4 team
  $s = New-Slide $pres 4
  Add-Header $s "TEAM R&R (팀원 역할)" "파트별로 무엇을 만들지 먼저 고정한다" "역할은 개발 중 바뀔 수 있지만, MVP 기준 책임 범위는 이 표를 따른다."
  Add-Panel $s 70 220 1140 410 0.03 $C.line | Out-Null
  $rows = @(
    @("기획 / PM", "천우영, 정동원", "MVP 우선순위, 마일스톤, 회의록, 게임 루프/낮밤 전환 구조"),
    @("플레이어 / HUD", "김성광", "1인칭 조작, 카메라, 체력/배고픔/스태미나, 기본 HUD"),
    @("채집 / 제작 / 건축", "최재호, 김동혁", "자원 상호작용, 리스폰, 도구/장비 티어, 고정 건축 지점"),
    @("전투 / AI / 보스", "김민석, 권성혁, 지재욱", "밤 웨이브, 몬스터 AI, 구조물 파괴, 최종 보스"),
    @("맵 / 밸런스 / QA", "강희원 + 전원", "자원량, 스폰 거리, 스탯 소모, 3일 단위 빌드 테스트")
  )
  for ($i=0; $i -lt $rows.Count; $i++) {
    $y = 246 + ($i * 70)
    if ($i -gt 0) { Add-Line $s 92 ($y-10) 1188 ($y-10) $C.line 1 $false | Out-Null }
    Add-Text $s $rows[$i][0] 100 $y 210 36 22 $C.ember $true 1 | Out-Null
    Add-Text $s $rows[$i][1] 338 $y 260 36 21 $C.ink $true 1 | Out-Null
    Add-Text $s $rows[$i][2] 620 $y 540 42 18 $C.mute $false 1 | Out-Null
  }
  Add-Footer $s 4

  # 5 timeline
  $s = New-Slide $pres 5
  Add-Header $s "FIRST 15 MINUTES (첫 15분)" "처음 한 판의 체크포인트" "첫 15분 안에 채집, 제작, 상자, 방어, 강화, 보스 접근을 모두 경험하게 만든다."
  Add-Panel $s 62 250 1160 300 0.02 $C.line | Out-Null
  Add-Line $s 130 374 1150 374 $C.line 4 $false | Out-Null
  $times = @("0:00","1:30","3:00","5:00","7:30","10:00","12:30","15:00")
  $labels = @("돌/가지 줍기`n1등급 채집", "돌 도구 제작`n첫 장비", "첫 상자 개방`n유물 1개", "1일차 밤`n방어 성공", "불꽃 강화`n3택1 선택", "2티어 접근`n장비 교체", "3일차 밤`n최종 웨이브", "보스 처치`n클리어")
  for ($i=0; $i -lt 8; $i++) {
    $x = 110 + ($i * 148)
    $color = $(if ($i % 2 -eq 0) { $C.ember } else { $C.cyan })
    Add-Text $s $times[$i] ($x-28) 300 90 28 20 $color $true 2 | Out-Null
    Add-Rect $s ($x+8) 362 20 20 $color 0 "none" $true | Out-Null
    Add-Text $s $labels[$i] ($x-48) 410 130 62 16 $C.ink $false 2 | Out-Null
  }
  Add-Text $s "완성 기준: 이 체크포인트가 한 판 안에서 자연스럽게 이어져야 한다." 170 592 940 34 23 $C.ember $true 2 | Out-Null
  Add-Footer $s 5

  # 6 flowchart
  $s = New-Slide $pres 6
  Add-Header $s "FLOWCHART (플로우차트)" "플레이 진행 순서" "GameFlowController는 순서를 알리고, 실제 행동은 각 컴포넌트가 처리한다."
  $nodes = @(
    @("게임 시작",80,252,150,80,$C.cyan),
    @("낮 시작`n파밍 / 제작 / 상자",285,252,190,80,$C.cyan),
    @("밤 시작`n웨이브 생성",540,252,190,80,$C.ember),
    @("아침 보상`n불꽃 강화 + 유물",795,252,210,80,$C.green),
    @("목표 일수`n도달?",1060,252,160,80,$C.ember)
  )
  foreach ($n in $nodes) { Add-BoxText $s $n[0] $n[1] $n[2] $n[3] $n[4] $n[5] $C.panel 20 $true 2 | Out-Null }
  Add-Line $s 230 292 285 292 $C.cyan 2 $true | Out-Null
  Add-Line $s 475 292 540 292 $C.cyan 2 $true | Out-Null
  Add-Line $s 730 292 795 292 $C.cyan 2 $true | Out-Null
  Add-Line $s 1005 292 1060 292 $C.cyan 2 $true | Out-Null
  Add-BoxText $s "아니오`n다음 날 반복" 800 430 205 78 $C.line $C.panel 19 $true 2 | Out-Null
  Add-BoxText $s "예`n보스 접근 3분" 1030 430 190 78 $C.red $C.panel 19 $true 2 | Out-Null
  Add-BoxText $s "보스 처치`n클리어" 820 570 180 72 $C.green $C.panel 19 $true 2 | Out-Null
  Add-BoxText $s "사망 또는`n불꽃 파괴`n실패" 1045 560 175 88 $C.red $C.panel 18 $true 2 | Out-Null
  Add-Line $s 1140 332 1140 430 $C.ember 2 $true | Out-Null
  Add-Line $s 1005 469 1030 469 $C.ember 2 $true | Out-Null
  Add-Line $s 902 508 902 570 $C.green 2 $true | Out-Null
  Add-Line $s 1133 508 1133 560 $C.red 2 $true | Out-Null
  Add-Line $s 800 469 150 469 $C.line 2 $false | Out-Null
  Add-Line $s 150 469 150 332 $C.line 2 $true | Out-Null
  Add-Text $s "반복 조건: 목표 일수 전이면 낮으로 돌아간다." 130 510 540 34 22 $C.mute $false 1 | Out-Null
  Add-Footer $s 6

  # 7 mindmap
  $s = New-Slide $pres 7
  Add-Header $s "MIND MAP (시스템 마인드맵)" "불꽃 심장을 중심으로 모든 기능을 연결한다" "각 기능은 따로 놀지 않고 불꽃 심장 방어 판단으로 이어져야 한다."
  $branches = @(
    @("플레이어", "이동 / 공격 / 장비`n체력 / 배고픔 / 스태미나", 92, 222, $C.cyan),
    @("낮 파밍", "자원 / 음식 / 코인`n거리별 등급", 90, 470, $C.green),
    @("거점 방어", "벽 / 포탑 / 함정`n고정 건축 지점", 480, 510, $C.ember),
    @("밤 웨이브", "스켈레톤 → 좀비 → 늑대`n보스 접근 3분", 882, 470, $C.red),
    @("보상", "상자 유물`n아침 불꽃 강화", 900, 222, $C.green),
    @("데이터", "JSON 로딩`nID 조회 / 밸런스 조정", 480, 180, $C.cyan)
  )
  $edges = @(
    @(490,348,382,271,$C.cyan),
    @(640,296,640,286,$C.cyan),
    @(790,348,890,271,$C.green),
    @(490,362,380,519,$C.green),
    @(640,408,620,500,$C.ember),
    @(790,362,872,519,$C.red)
  )
  foreach ($e in $edges) { Add-Line $s $e[0] $e[1] $e[2] $e[3] $e[4] 2 $false | Out-Null }
  Add-BoxText $s "불꽃 심장`n보호 대상 / 방어 무기" 500 304 280 96 $C.ember $C.panel 23 $true 2 | Out-Null
  foreach ($b in $branches) {
    Add-BoxText $s ($b[0] + "`n" + $b[1]) $b[2] $b[3] 280 98 $b[4] $C.panel 18 $true 2 | Out-Null
  }
  Add-Footer $s 7

  # 8 flame heart
  $s = New-Slide $pres 8
  Add-Image $s (MoreImg "ck-more-01-flame-heart-close.png") 700 96 500 281 | Out-Null
  Add-Header $s "FLAME HEART (불꽃 심장)" "불꽃 심장은 지켜야 할 목표이자 기본 방어 장치다" "플레이어와 거점이 함께 강해져야 방어 게임의 목적이 분명해진다."
  Add-Panel $s 80 250 560 330 0.02 $C.line | Out-Null
  Add-Text $s "체력" 112 290 120 30 26 $C.green $true 1 | Out-Null
  Add-Text $s "0이 되면 즉시 실패. 업그레이드로 최대 체력이 증가한다." 250 292 340 34 19 $C.ink $false 1 | Out-Null
  Add-Text $s "온기 범위" 112 378 150 30 26 $C.ember $true 1 | Out-Null
  Add-Text $s "범위 안 적에게 지속 피해. 보스에게도 적용되지만 효과는 줄어든다." 250 378 345 48 19 $C.ink $false 1 | Out-Null
  Add-Text $s "방어 레이저" 112 480 170 30 26 $C.cyan $true 1 | Out-Null
  Add-Text $s "초기에는 약하다. 같은 대상을 계속 쏘면 피해가 올라간다." 250 482 345 48 19 $C.ink $false 1 | Out-Null
  Add-Text $s "아침 강화 3택1은 플레이어 스탯이 아니라 불꽃 전용 강화만 제공한다." 700 420 500 64 23 $C.ember $true 1 | Out-Null
  Add-Footer $s 8

  # 9 day gathering
  $s = New-Slide $pres 9
  Add-Image $s (MoreImg "ck-more-02-day-gathering.png") 680 110 500 281 | Out-Null
  Add-Header $s "DAY GATHERING (낮 파밍)" "낮에도 안전하지 않지만, 밤보다 훨씬 덜 위험하다" "낮은 다음 밤을 버티기 위해 자원과 성장 재료를 모으는 시간이다."
  Add-Panel $s 80 250 540 330 0.02 $C.line | Out-Null
  Add-Text $s "자원" 112 288 140 30 26 $C.cyan $true 1 | Out-Null
  Add-Text $s "나무, 돌, 광석은 거점에서 멀수록 높은 등급이 나온다." 260 290 310 42 19 $C.ink $false 1 | Out-Null
  Add-Text $s "음식" 112 380 140 30 26 $C.green $true 1 | Out-Null
  Add-Text $s "얼어붙은 쥐고기 3종: 체력 회복, 스태미나 회복, 허기 회복." 260 380 320 48 19 $C.ink $false 1 | Out-Null
  Add-Text $s "코인" 112 484 140 30 26 $C.ember $true 1 | Out-Null
  Add-Text $s "몬스터 처치로 획득한다. 상자는 코인으로만 연다." 260 486 320 42 19 $C.ink $false 1 | Out-Null
  Add-Text $s "순록은 거점 근처에 주기적으로 등장하고, 사냥해 고기를 얻는다." 700 430 450 54 22 $C.mute $false 1 | Out-Null
  Add-Footer $s 9

  # 10 resource gear
  $s = New-Slide $pres 10
  Add-Header $s "RESOURCE & GEAR (자원과 장비)" "티어는 막는 문이 아니라 채집 속도 차이다" "상위 자원을 낮은 장비로도 캘 수 있지만 매우 느리다. 위험을 감수하면 먼저 열 수 있다."
  Add-Panel $s 70 228 1140 360 0.02 $C.line | Out-Null
  $rows2 = @(
    @("광석", @("돌","철","미스릴","아다만티움"), $C.cyan, 270),
    @("무기", @("돌칼","철칼","미스릴칼","아다만티움 칼"), $C.ember, 370),
    @("방어구", @("돌 방어구","철 방어구","미스릴 방어구","아다만티움 방어구"), $C.green, 470)
  )
  foreach ($r in $rows2) {
    Add-Text $s $r[0] 112 $r[3] 140 32 25 $r[2] $true 1 | Out-Null
    for ($i=0; $i -lt 4; $i++) {
      $x = 290 + ($i * 210)
      Add-BoxText $s $r[1][$i] $x ($r[3]-8) 150 50 $r[2] $C.panel 18 $true 2 | Out-Null
      if ($i -lt 3) { Add-Line $s ($x+150) ($r[3]+17) ($x+205) ($r[3]+17) $C.line 2 $true | Out-Null }
    }
  }
  Add-Text $s "도구 흐름: 돌/가지 줍기 → 돌 도끼/곡괭이 제작 → 철 접근 → 다음 장비 제작" 120 622 1040 32 22 $C.ember $true 2 | Out-Null
  Add-Footer $s 10

  # 11 reward
  $s = New-Slide $pres 11
  Add-Image $s (Img "cinderkeep-ingame-mucklike-02-chest-relic.png") 720 136 440 248 | Out-Null
  Add-Header $s "CHEST & RELIC (상자와 유물)" "성장은 화면에서 바로 확인되어야 한다" "상자 유물은 플레이어 강화, 아침 선택은 불꽃 강화로 역할을 나눈다."
  Add-Panel $s 82 250 560 330 0.02 $C.line | Out-Null
  Add-Text $s "상자" 116 292 140 30 26 $C.ember $true 1 | Out-Null
  Add-Text $s "코인으로만 개방한다. 이동 속도, 공격 속도, 체력 같은 영구 유물을 지급한다." 252 292 340 54 19 $C.ink $false 1 | Out-Null
  Add-Text $s "유물 표시" 116 398 160 30 26 $C.cyan $true 1 | Out-Null
  Add-Text $s "획득 즉시 화면 하단에 아이콘 누적. 마우스 오버 시 수치 설명을 보여준다." 252 398 340 54 19 $C.ink $false 1 | Out-Null
  Add-Text $s "아침 보상" 116 504 170 30 26 $C.green $true 1 | Out-Null
  Add-Text $s "시간 정지 후 불꽃 강화 3택1 화면과 랜덤 유물 지급 화면을 분리한다." 252 504 340 54 19 $C.ink $false 1 | Out-Null
  Add-Text $s "예시 표시: +5 공격력 / +5% 이동속도 / 최대 체력 증가" 708 438 470 44 22 $C.ember $true 1 | Out-Null
  Add-Footer $s 11

  # 12 base defense
  $s = New-Slide $pres 12
  Add-Image $s (MoreImg "ck-more-03-fixed-build-site.png") 704 122 500 281 | Out-Null
  Add-Header $s "BASE BUILDING (거점 건축)" "MVP는 고정 건축 지점으로 만든다" "자유 건축은 확장으로 남기고, 첫 버전은 정해진 자리에서 건설/수리/업그레이드를 검증한다."
  Add-Panel $s 80 250 560 330 0.02 $C.line | Out-Null
  Add-Text $s "건설 방식" 112 290 180 30 25 $C.cyan $true 1 | Out-Null
  Add-Text $s "처음부터 모든 건축 지점이 보인다. 자원을 들고 E를 길게 눌러 건설한다." 300 292 290 54 19 $C.ink $false 1 | Out-Null
  Add-Text $s "피격 처리" 112 390 180 30 25 $C.ember $true 1 | Out-Null
  Add-Text $s "건설 중 맞아도 취소되지 않는다. 대신 건설 속도가 느려진다." 300 392 300 48 19 $C.ink $false 1 | Out-Null
  Add-Text $s "부서진 뒤" 112 488 180 30 25 $C.green $true 1 | Out-Null
  Add-Text $s "잔해가 남고 같은 자리에서 수리/재건축한다." 300 490 300 44 19 $C.ink $false 1 | Out-Null
  Add-Text $s "벽 / 포탑 / 함정은 모두 업그레이드 가능. 베이스가 점점 단단해지는 느낌을 만든다." 710 438 470 64 22 $C.ember $true 1 | Out-Null
  Add-Footer $s 12

  # 13 enemy
  $s = New-Slide $pres 13
  Add-Image $s (MoreImg "ck-more-05-boss-wave.png") 700 118 500 281 | Out-Null
  Add-Header $s "ENEMY WAVE (적 웨이브)" "적은 밤마다 몰려오고, 최종 보스만 잡으면 클리어한다" "몬스터 종류는 밤마다 추가된다. 2일차에는 1일차 몬스터도 계속 나온다."
  Add-Panel $s 82 250 560 330 0.02 $C.line | Out-Null
  $erows = @(
    @("1일차", "스켈레톤", "느린 근접. 수로 압박한다.", $C.ember),
    @("2일차", "+ 얼음 좀비", "체력이 높고 구조물 파괴가 강하다.", $C.cyan),
    @("3일차", "+ 얼음늑대", "빠르게 플레이어를 물고 늘어진다.", $C.green),
    @("최종", "보스", "불꽃 심장으로 걸어오고, 처치하면 클리어.", $C.red)
  )
  for ($i=0; $i -lt 4; $i++) {
    $y = 286 + ($i * 68)
    Add-Text $s $erows[$i][0] 112 $y 100 30 22 $erows[$i][3] $true 1 | Out-Null
    Add-Text $s $erows[$i][1] 240 $y 190 30 23 $C.ink $true 1 | Out-Null
    Add-Text $s $erows[$i][2] 438 $y 170 44 16 $C.mute $false 1 | Out-Null
  }
  Add-Text $s "3일차 밤에는 일반 몬스터가 계속 나오며, 보스가 3분 동안 불꽃 심장으로 접근한다." 704 430 500 62 22 $C.ink $true 1 | Out-Null
  Add-Footer $s 13

  # 14 targeting
  $s = New-Slide $pres 14
  Add-Header $s "TARGETING RULE (AI 타깃 규칙)" "적은 불꽃 심장으로 가는 길을 먼저 찾는다" "구조물 공격 규칙을 명확히 해야 포탑과 벽의 가치가 생긴다."
  Add-Panel $s 72 228 1136 360 0.02 $C.line | Out-Null
  Add-BoxText $s "목표: 불꽃 심장" 112 300 200 74 $C.ember $C.panel 20 $true 2 | Out-Null
  Add-Line $s 312 337 390 337 $C.cyan 2 $true | Out-Null
  Add-BoxText $s "경로가 열림" 390 260 190 64 $C.green $C.panel 19 $true 2 | Out-Null
  Add-BoxText $s "경로가 막힘" 390 386 190 64 $C.red $C.panel 19 $true 2 | Out-Null
  Add-Line $s 580 292 690 292 $C.green 2 $true | Out-Null
  Add-Line $s 580 418 690 418 $C.red 2 $true | Out-Null
  Add-BoxText $s "이동 후 공격" 690 260 190 64 $C.green $C.panel 19 $true 2 | Out-Null
  Add-BoxText $s "근처 구조물 파괴" 690 386 220 64 $C.red $C.panel 19 $true 2 | Out-Null
  Add-Line $s 910 418 1010 418 $C.red 2 $true | Out-Null
  Add-BoxText $s "길이 열리면`n다시 심장으로 이동" 1010 370 170 96 $C.ember $C.panel 18 $true 2 | Out-Null
  Add-Text $s "포탑은 처음부터 최우선 타깃이 아니다. 맞거나 길을 막는 상황에서 파괴 대상이 된다.`n보스는 함정 감속/기절을 받지만 일반 몬스터보다 효과가 약하다." 160 608 960 54 20 $C.mute $false 2 | Out-Null
  Add-Footer $s 14

  # 15 data
  $s = New-Slide $pres 15
  Add-Header $s "DATA DRIVEN (JSON 데이터 구성)" "수치는 JSON 로딩과 ID 조회로 연결한다" "팀원이 밸런스를 바꿀 때 코드 수정 없이 데이터 값을 조정할 수 있게 만든다."
  Add-Panel $s 72 228 1136 360 0.02 $C.line | Out-Null
  Add-Text $s "Static Data" 110 260 260 34 26 $C.cyan $true 1 | Out-Null
  Add-Text $s "ResourceData`nRecipeData`nRelicData`nEnemyData`nFlameUpgradeData`nBuildingData" 126 310 260 190 21 $C.ink $true 1 | Out-Null
  Add-Text $s "Schema 예시" 480 260 260 34 26 $C.ember $true 1 | Out-Null
  Add-Text $s "id, displayName, tier`nprefabKey, iconKey`ngatherTime, respawnSec`ncostItemIds, rewardIds`nhp, damage, moveSpeed" 496 310 330 160 20 $C.ink $false 1 | Out-Null
  Add-Text $s "Manager 연결" 900 260 260 34 26 $C.green $true 1 | Out-Null
  Add-Text $s "Excel → JSON`nGameDataManager 로드`nID로 데이터 조회`n컴포넌트가 수치 사용`nGameManager는 진행 상태 관리" 884 310 300 180 19 $C.ink $false 1 | Out-Null
  Add-Text $s "예: EnemySpawner는 enemyId만 들고 있고, 실제 체력/속도/드롭 코인은 EnemyData에서 읽는다." 150 610 980 36 21 $C.mute $false 2 | Out-Null
  Add-Footer $s 15

  # 16 class diagram
  $s = New-Slide $pres 16
  Add-Header $s "CLASS DIAGRAM (클래스 다이어그램)" "실제로 구현할 역할 분리" "Controller는 순서만 관리하고, 각 오브젝트는 자기 역할 컴포넌트를 가진다."
  $cls = @(
    @("GameManager`n날짜 / 승패 / 진행 Model", 74, 218, 245, 82, $C.green),
    @("GameFlowController`n낮밤 순서 / 보스 시작 알림", 392, 218, 280, 82, $C.ember),
    @("GameDataManager`nJSON 로드 / ID 조회", 746, 218, 245, 82, $C.cyan),
    @("Player`nMovement / Combat`nInventory / HUD", 74, 420, 245, 104, $C.cyan),
    @("FlameHeart`nHealth / WarmthAura`nBeam / Upgrade", 392, 420, 280, 104, $C.ember),
    @("WorldObject`nGatherable / Chest`nBuildSite / Structure", 746, 420, 245, 104, $C.green),
    @("Enemy`nTargetSelector`nMove / Attack / Drop", 1038, 420, 170, 104, $C.red)
  )
  foreach ($c1 in $cls) { Add-BoxText $s $c1[0] $c1[1] $c1[2] $c1[3] $c1[4] $c1[5] $C.panel 17 $true 2 | Out-Null }
  Add-Line $s 319 259 392 259 $C.line 2 $true | Out-Null
  Add-Line $s 672 259 746 259 $C.line 2 $true | Out-Null
  Add-Line $s 532 300 532 420 $C.line 2 $true | Out-Null
  Add-Line $s 868 300 868 420 $C.line 2 $true | Out-Null
  Add-Line $s 672 472 746 472 $C.line 2 $true | Out-Null
  Add-Line $s 991 472 1038 472 $C.line 2 $true | Out-Null
  Add-Text $s "Inspector 참조가 많아지는 Controller는 분리한다. UI는 미리 만든 Prefab을 Instantiate한다." 120 610 1040 38 20 $C.mute $false 2 | Out-Null
  Add-Footer $s 16

  # 17 implementation
  $s = New-Slide $pres 17
  Add-Header $s "SYSTEM SHAPE (구현 기준)" "기능은 작게 나누고, 데이터로 수치를 바꾼다" "팀원이 같은 방식으로 코드를 짜도록 최소 규칙을 맞춘다."
  Add-Panel $s 80 230 1120 340 0.02 $C.line | Out-Null
  Add-Text $s "Controller" 124 270 220 32 26 $C.ember $true 1 | Out-Null
  Add-Text $s "낮/밤 전환, 보상 시작, 보스 시작처럼 순서만 알린다." 368 272 720 32 20 $C.ink $false 1 | Out-Null
  Add-Text $s "Component" 124 350 220 32 26 $C.cyan $true 1 | Out-Null
  Add-Text $s "이동, 채집, 공격, 드롭, 건축 게이지는 해당 오브젝트가 직접 처리한다." 368 352 760 32 20 $C.ink $false 1 | Out-Null
  Add-Text $s "Manager" 124 430 220 32 26 $C.green $true 1 | Out-Null
  Add-Text $s "GameDataManager는 기획 수치를 읽고, GameManager는 현재 진행 상태를 가진다." 368 432 760 32 20 $C.ink $false 1 | Out-Null
  Add-Text $s "UI" 124 510 220 32 26 $C.red $true 1 | Out-Null
  Add-Text $s "런타임에서 빈 GameObject를 새로 만들지 않고, 미리 만든 UI Prefab을 생성한다." 368 512 760 32 20 $C.ink $false 1 | Out-Null
  Add-Footer $s 17

  # 18 roadmap
  $s = New-Slide $pres 18
  Add-Header $s "ROADMAP (개발 로드맵)" "6/16 - 7/3 MVP, 이후 7일 루프로 확장" "MVP 3일/15분 루프를 안정화한 뒤 7일 생존과 30분 이상 플레이로 늘린다."
  Add-Panel $s 64 250 1150 280 0.02 $C.line | Out-Null
  $phases = @(
    @("6/16-6/18", "기초 루프", "이동/시점`n낮밤 타이머`n자원 채집"),
    @("6/19-6/22", "성장", "상자/코인/유물`n장비 슬롯`n허기/스태미나"),
    @("6/23-6/26", "거점 방어", "불꽃 심장`n고정 건축 지점`n벽/포탑/함정"),
    @("6/27-6/30", "전투", "3종 몬스터`nAI 타깃팅`n보스 접근/처치"),
    @("7/1-7/3", "폴리싱", "밸런스`nUI 피드백`n15분 검증")
  )
  for ($i=0; $i -lt 5; $i++) {
    $x = 94 + ($i * 226)
    $color = $(if ($i -lt 2) { $C.cyan } elseif ($i -lt 4) { $C.ember } else { $C.green })
    Add-Text $s $phases[$i][0] $x 284 160 28 18 $color $true 2 | Out-Null
    Add-Rect $s ($x+16) 324 130 16 $color 0 "none" $true | Out-Null
    Add-Text $s $phases[$i][1] ($x-10) 370 180 36 24 $C.ink $true 2 | Out-Null
    Add-Text $s $phases[$i][2] ($x-8) 420 176 82 16 $C.mute $false 2 | Out-Null
    if ($i -lt 4) { Add-Line $s ($x+160) 332 ($x+204) 332 $C.line 2 $true | Out-Null }
  }
  Add-Text $s "확장 목표: 7일차까지 이어지는 장기 루프, 자유 건축, 추가 보스, 희귀 장비, 추위 게이지." 126 604 1030 34 22 $C.ember $true 2 | Out-Null
  Add-Footer $s 18

  # disable proofing attempt for all shapes
  foreach ($sl in $pres.Slides) {
    foreach ($sh in $sl.Shapes) {
      try {
        if ($sh.HasTextFrame -and $sh.TextFrame.HasText) {
          Set-NoProof $sh.TextFrame.TextRange
        }
      } catch {}
    }
  }

  $pres.SaveAs($out, 24)
  $pres.Export($preview, "PNG", $W, $H)
  $pres.Close()
}
finally {
  if ($pres) { try { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($pres) | Out-Null } catch {} }
  if ($pp) { try { $pp.Quit() } catch {}; try { [System.Runtime.InteropServices.Marshal]::ReleaseComObject($pp) | Out-Null } catch {} }
  [GC]::Collect()
  [GC]::WaitForPendingFinalizers()
}

Write-Output "saved $out"
'''

path = Path("tools/build_cinderkeep_gdp_v2.ps1")
path.write_text(ps, encoding="utf-16")
print(path)
