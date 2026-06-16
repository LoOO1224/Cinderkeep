import fs from "node:fs/promises";
import path from "node:path";
import { Presentation, PresentationFile } from "file:///C:/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs";

const OUT = "C:\\Users\\user\\Desktop\\Downloads\\Cinderkeep_GDP_V1.pptx";
const ASSET_DIR = "C:\\Users\\user\\Desktop\\Downloads\\CK 게임 컨셉";
const WORK = "C:\\Users\\user\\AppData\\Local\\Temp\\cinderkeep-gdp-v1";
const PREVIEW = path.join(WORK, "preview");
const LAYOUT = path.join(WORK, "layout");
const QA = path.join(WORK, "qa");

const W = 1280;
const H = 720;
const C = {
  ink: "#F5F7FA",
  mute: "#B8C7D8",
  dim: "#7890A7",
  bg: "#07111D",
  panel: "#102235",
  panel2: "#152B42",
  line: "#31516D",
  ember: "#FF9D2E",
  flame: "#FF5A1E",
  cyan: "#63C7E8",
  green: "#37D39A",
  red: "#EF4B55",
  brown: "#2A1710",
  black: "#000000",
  white: "#FFFFFF",
};
const FONT = "Malgun Gothic";

const imgs = {
  menu: "cinderkeep-menu-concept-01-flame-heart-guardian.png",
  day: "cinderkeep-ingame-mucklike-01-day-gathering.png",
  chest: "cinderkeep-ingame-mucklike-02-chest-relic.png",
  night: "cinderkeep-ingame-mucklike-03-night-defense.png",
  boss: "cinderkeep-ingame-mucklike-04-boss-approach.png",
};

async function imageBlob(name) {
  const file = path.join(ASSET_DIR, name);
  const bytes = await fs.readFile(file);
  return bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength);
}

async function addImage(slide, name, pos, opacity = 1) {
  slide.images.add({
    blob: await imageBlob(name),
    contentType: "image/png",
    alt: name,
    fit: "cover",
    opacity,
    position: pos,
  });
}

function addRect(slide, pos, fill, line = "none", radius = "rounded-lg", opacity = 1) {
  return slide.shapes.add({
    geometry: "roundRect",
    position: pos,
    fill,
    opacity,
    line: { style: "solid", fill: line, width: line === "none" ? 0 : 1.5 },
    borderRadius: radius,
  });
}

function addText(slide, text, pos, opt = {}) {
  const t = slide.shapes.add({
    geometry: "textbox",
    position: pos,
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  t.text = text;
  t.text.style = {
    fontSize: opt.size ?? 24,
    bold: opt.bold ?? false,
    color: opt.color ?? C.ink,
    typeface: opt.typeface ?? FONT,
    alignment: opt.align ?? "left",
    verticalAlignment: opt.valign ?? "top",
  };
  return t;
}

function addTitle(slide, eyebrow, title, subtitle) {
  addText(slide, eyebrow, { left: 72, top: 42, width: 420, height: 28 }, { size: 13, bold: true, color: C.ember });
  addText(slide, title, { left: 72, top: 82, width: 860, height: 68 }, { size: 42, bold: true });
  if (subtitle) addText(slide, subtitle, { left: 74, top: 152, width: 980, height: 44 }, { size: 18, color: C.mute });
}

function addFooter(slide, n) {
  addText(slide, `Cinderkeep GDP V1  |  ${String(n).padStart(2, "0")}`, { left: 72, top: 686, width: 300, height: 20 }, { size: 10, color: C.dim });
}

function line(slide, x1, y1, x2, y2, color = C.cyan, w = 2) {
  slide.shapes.add({
    geometry: "line",
    position: { left: x1, top: y1, width: x2 - x1, height: y2 - y1 },
    fill: "none",
    line: { style: "solid", fill: color, width: w },
  });
}

function arrowText(slide, x, y, color = C.cyan) {
  addText(slide, "→", { left: x, top: y, width: 34, height: 34 }, { size: 27, bold: true, color, align: "center" });
}

function addMetric(slide, label, value, x, accent) {
  addText(slide, value, { left: x, top: 212, width: 230, height: 70 }, { size: 48, bold: true, color: accent, align: "center" });
  addText(slide, label, { left: x, top: 286, width: 230, height: 42 }, { size: 17, color: C.mute, align: "center" });
}

function addBullet(slide, title, body, x, y, w, accent) {
  addText(slide, title, { left: x, top: y, width: w, height: 32 }, { size: 22, bold: true, color: accent });
  addText(slide, body, { left: x, top: y + 42, width: w, height: 92 }, { size: 18, color: C.ink });
}

function addNode(slide, label, pos, accent = C.cyan, fill = C.panel) {
  addRect(slide, pos, fill, accent, "rounded-md", 0.96);
  addText(slide, label, { left: pos.left + 12, top: pos.top + 14, width: pos.width - 24, height: pos.height - 28 }, { size: 18, bold: true, align: "center", valign: "mid" });
}

function addBg(slide) {
  slide.background.fill = C.bg;
}

async function build() {
  await fs.mkdir(PREVIEW, { recursive: true });
  await fs.mkdir(LAYOUT, { recursive: true });
  await fs.mkdir(QA, { recursive: true });

  const p = Presentation.create({ slideSize: { width: W, height: H } });

  // 1 cover
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.menu, { left: 0, top: 0, width: W, height: H });
    s.shapes.add({ geometry: "rect", position: { left: 0, top: 0, width: W, height: H }, fill: "#000000", opacity: 0.42, line: { style: "solid", fill: "none", width: 0 } });
    addText(s, "GAME DESIGN PROPOSAL", { left: 72, top: 54, width: 420, height: 28 }, { size: 14, bold: true, color: C.ember });
    addText(s, "Cinderkeep", { left: 72, top: 132, width: 720, height: 88 }, { size: 68, bold: true });
    addText(s, "잿불 성채: 마지막 불씨", { left: 76, top: 228, width: 760, height: 52 }, { size: 36, bold: true, color: C.ink });
    addText(s, "Muck의 빠른 성장감을 거점 방어 목표로 묶은 1인칭 생존 로그라이트", { left: 78, top: 330, width: 760, height: 68 }, { size: 24, color: C.mute });
    addText(s, "낮에는 파밍하고, 밤에는 불꽃 심장을 지킨다. 3일차 밤에는 최종 보스가 3분 동안 걸어온다.", { left: 78, top: 606, width: 900, height: 44 }, { size: 18, color: C.ink });
    addFooter(s, 1);
  }

  // 2 high concept
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.day, { left: 760, top: 0, width: 520, height: H }, 0.95);
    s.shapes.add({ geometry: "rect", position: { left: 720, top: 0, width: 560, height: H }, fill: "#000000", opacity: 0.3, line: { style: "solid", fill: "none", width: 0 } });
    addTitle(s, "HIGH CONCEPT", "게임을 한 문장으로", "빠른 파밍과 상자 성장의 쾌감을 불꽃 심장 방어라는 단일 목표로 압축한다.");
    addMetric(s, "낮/밤 한 사이클", "5분", 80, C.cyan);
    addMetric(s, "목표 플레이타임", "15분", 330, C.ember);
    addMetric(s, "MVP 생존 목표", "3일", 580, C.green);
    addBullet(s, "남기는 것", "즉시 채집, 빠른 장비 제작, 코인 상자, 유물 누적, 점점 커지는 전투 밀도", 88, 382, 520, C.green);
    addBullet(s, "바꾸는 것", "무작정 오래 버티는 게임이 아니라, 불꽃 심장과 방어선을 중심으로 판단하게 만든다.", 88, 520, 520, C.ember);
    addFooter(s, 2);
  }

  // 3 game pillars
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.night, { left: 0, top: 0, width: W, height: H }, 0.35);
    addTitle(s, "DESIGN PILLARS", "팀이 흔들리면 이 4개로 돌아온다", "모든 시스템은 첫 15분의 압축된 성장감과 거점 방어 판단을 강화해야 한다.");
    const xs = [84, 352, 620, 888];
    const titles = ["빠른 손맛", "보이는 성장", "거점 압박", "소프트락"];
    const bodies = [
      "처음 1분 안에 채집, 제작, 첫 전투가 모두 발생한다.",
      "유물 아이콘과 장비 티어가 화면에 쌓이며 강해진 이유가 보인다.",
      "불꽃 심장 체력, 벽 파손, 포탑 위치가 선택의 중심이 된다.",
      "티어 제한은 막는 게 아니라 느리게 열어 위험 감수를 유도한다.",
    ];
    const colors = [C.cyan, C.ember, C.flame, C.green];
    xs.forEach((x, i) => {
      addText(s, `0${i + 1}`, { left: x, top: 240, width: 70, height: 60 }, { size: 44, bold: true, color: colors[i] });
      addText(s, titles[i], { left: x, top: 320, width: 220, height: 36 }, { size: 26, bold: true });
      addText(s, bodies[i], { left: x, top: 372, width: 220, height: 126 }, { size: 18, color: C.mute });
    });
    addFooter(s, 3);
  }

  // 4 MVP scope
  {
    const s = p.slides.add(); addBg(s);
    addTitle(s, "MVP SCOPE", "이번 버전에서 만들 것 / 만들지 않을 것", "확장 가능성은 열어두되, 첫 구현은 고정 건축 지점과 3일 생존 루프로 잠근다.");
    addText(s, "반드시 만든다", { left: 92, top: 230, width: 360, height: 40 }, { size: 30, bold: true, color: C.green });
    addText(s, "나중에 확장한다", { left: 732, top: 230, width: 360, height: 40 }, { size: 30, bold: true, color: C.dim });
    const must = [
      "1인칭 이동/시점/손 장비 표시",
      "낮 3분 / 밤 2분 / 아침 보상 15초",
      "3일차 밤 보스 접근 3분 + 처치 클리어",
      "불꽃 심장 체력, 온기 피해, 방어 레이저",
      "고정 건축 지점: 벽, 포탑, 함정",
      "상자 유물, 무기/방패/신발 장비 효과",
    ];
    const later = [
      "자유 건축과 회수/이동/회전 편집",
      "희귀 장비와 세트 옵션",
      "추위 게이지와 방한 장비",
      "30분 이상 장기 모드와 추가 보스",
      "원거리 탄약/화살 제작",
      "자동 장착과 세부 장비 슬롯 효과",
    ];
    addText(s, must.map(v => `• ${v}`).join("\n"), { left: 96, top: 292, width: 500, height: 300 }, { size: 21, color: C.ink });
    addText(s, later.map(v => `• ${v}`).join("\n"), { left: 736, top: 292, width: 470, height: 300 }, { size: 21, color: C.mute });
    line(s, 640, 236, 640, 610, C.line, 2);
    addFooter(s, 4);
  }

  // 5 first 15 minutes
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.chest, { left: 0, top: 428, width: W, height: 292 }, 0.42);
    addTitle(s, "FIRST 15 MINUTES", "도파민 배치표", "플레이어가 '한 판 더'를 느끼려면 15분 안에 성장 이유와 방어 압박을 모두 체험해야 한다.");
    const steps = [
      ["0:00", "돌/가지 줍기\n1등급 채집 시작"],
      ["1:30", "돌 도끼/곡괭이\n첫 장비 제작"],
      ["3:00", "첫 상자 오픈\n유물 1개 획득"],
      ["5:00", "1일차 밤\n첫 방어 성공"],
      ["7:30", "불꽃 업그레이드\n3택1 선택"],
      ["10:00", "2티어 자원 접근\n장비 교체"],
      ["12:30", "3일차 밤 진입\n최종 웨이브"],
      ["15:00", "보스 처치\n생존 클리어"],
    ];
    const y = 288, x0 = 82, gap = 148;
    line(s, 110, y + 52, 1168, y + 52, C.line, 4);
    steps.forEach((st, i) => {
      const x = x0 + i * gap;
      addText(s, st[0], { left: x - 30, top: y, width: 90, height: 30 }, { size: 20, bold: true, color: i % 2 ? C.cyan : C.ember, align: "center" });
      addRect(s, { left: x, top: y + 42, width: 18, height: 18 }, i % 2 ? C.cyan : C.ember, "none", "rounded-sm");
      addText(s, st[1], { left: x - 46, top: y + 82, width: 120, height: 82 }, { size: 15, color: C.ink, align: "center" });
    });
    addFooter(s, 5);
  }

  // 6 run loop
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.day, { left: 0, top: 0, width: 640, height: H }, 0.3);
    await addImage(s, imgs.night, { left: 640, top: 0, width: 640, height: H }, 0.3);
    addTitle(s, "CORE LOOP", "낮과 밤의 역할", "낮은 성장 시간을 주고, 밤은 그 성장 판단이 맞았는지 검증한다.");
    const nodes = [
      ["낮 3분", "파밍 / 상자 / 제작\n낮 몬스터는 적지만 계속 증가", 88, 286, C.cyan],
      ["밤 2분", "불꽃 심장 방어\n웨이브와 구조물 파손 압박", 390, 286, C.ember],
      ["아침 15초", "시간 정지\n불꽃 강화 선택 + 유물 지급", 692, 286, C.green],
      ["3일차 밤", "보스 접근 3분\n처치 시 생존 클리어", 994, 286, C.red],
    ];
    nodes.forEach(([a, b, x, y, col]) => {
      addText(s, a, { left: x, top: y, width: 210, height: 42 }, { size: 28, bold: true, color: col, align: "center" });
      addText(s, b, { left: x - 24, top: y + 58, width: 258, height: 84 }, { size: 17, color: C.ink, align: "center" });
    });
    [320, 622, 924].forEach(x => arrowText(s, x, 336));
    addFooter(s, 6);
  }

  // 7 full flowchart
  {
    const s = p.slides.add(); addBg(s);
    addTitle(s, "FULL FLOWCHART", "실제 플레이 진행 순서", "GameFlowController는 순서만 알리고, 행동은 Player / FlameHeart / WorldObject / Enemy 컴포넌트가 처리한다.");
    const row1 = [
      ["게임 시작", 60, 250, C.cyan],
      ["낮 시작\n파밍 / 제작 / 상자", 250, 250, C.cyan],
      ["밤 시작\n웨이브 생성", 500, 250, C.ember],
      ["아침 보상\n강화 선택", 750, 250, C.green],
      ["목표 일수?", 1000, 250, C.ember],
    ];
    row1.forEach(([t, x, y, col]) => addNode(s, t, { left: x, top: y, width: 170, height: 92 }, col));
    [220, 470, 720, 970].forEach(x => arrowText(s, x, 282));
    addNode(s, "아니오\n다음 날 반복", { left: 890, top: 430, width: 190, height: 86 }, C.dim);
    addNode(s, "예\n보스 접근 3분", { left: 1090, top: 430, width: 150, height: 86 }, C.red);
    addNode(s, "보스 처치\n클리어", { left: 914, top: 565, width: 150, height: 74 }, C.green);
    addNode(s, "사망 또는\n불꽃 파괴\n실패", { left: 1060, top: 552, width: 180, height: 90 }, C.red);
    line(s, 1085, 342, 1085, 445, C.ember, 2);
    line(s, 1080, 474, 1090, 474, C.ember, 2);
    arrowText(s, 1054, 454, C.ember);
    line(s, 988, 516, 988, 565, C.green, 2);
    line(s, 1162, 516, 1162, 552, C.red, 2);
    addText(s, "반복 조건: 목표 일수 전이면 낮으로 복귀", { left: 132, top: 470, width: 540, height: 36 }, { size: 21, color: C.mute });
    line(s, 890, 472, 122, 472, C.line, 2);
    line(s, 122, 472, 122, 342, C.line, 2);
    addFooter(s, 7);
  }

  // 8 flame heart
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.night, { left: 0, top: 0, width: W, height: H }, 0.45);
    addTitle(s, "FLAME HEART", "불꽃 심장은 보호 대상이자 방어 무기", "플레이어가 강해지는 동시에 베이스도 강해져야 '내 거점이 버틴다'는 감각이 생긴다.");
    addText(s, "체력", { left: 130, top: 292, width: 180, height: 36 }, { size: 28, bold: true, color: C.green, align: "center" });
    addText(s, "0이 되면 즉시 실패.\n업그레이드로 최대 체력 증가.", { left: 82, top: 342, width: 276, height: 86 }, { size: 19, align: "center" });
    addText(s, "온기 범위", { left: 550, top: 250, width: 180, height: 36 }, { size: 28, bold: true, color: C.ember, align: "center" });
    addText(s, "범위 안 적에게 지속 피해.\n보스에게는 적용되지만 효율 감소.", { left: 500, top: 300, width: 290, height: 86 }, { size: 19, align: "center" });
    addText(s, "방어 레이저", { left: 938, top: 292, width: 220, height: 36 }, { size: 28, bold: true, color: C.cyan, align: "center" });
    addText(s, "초기 약함.\n같은 목표를 계속 쏘면 피해 증가.", { left: 908, top: 342, width: 280, height: 86 }, { size: 19, align: "center" });
    line(s, 365, 360, 500, 326, C.ember, 3);
    line(s, 790, 326, 912, 360, C.cyan, 3);
    addFooter(s, 8);
  }

  // 9 growth/rewards
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.chest, { left: 720, top: 0, width: 560, height: H }, 0.55);
    addTitle(s, "GROWTH & REWARD", "성장은 화면에서 바로 보여야 한다", "유물은 플레이어 영구 강화, 불꽃 업그레이드는 불꽃 전용 강화로 분리한다.");
    addBullet(s, "상자", "코인으로만 개방. 이동 속도, 공격 속도, 체력 등 영구 유물을 즉시 지급한다.", 86, 245, 520, C.ember);
    addBullet(s, "유물 표시", "획득 즉시 화면 하단에 아이콘 누적. 마우스 오버 시 +5 공격력, +5% 이동속도처럼 수치 표시.", 86, 390, 560, C.cyan);
    addBullet(s, "아침 보상", "밤을 넘기면 시간 정지. 불꽃 강화 3택1 화면과 랜덤 유물 지급 화면을 분리한다.", 86, 535, 560, C.green);
    addFooter(s, 9);
  }

  // 10 resources/equipment
  {
    const s = p.slides.add(); addBg(s);
    addTitle(s, "RESOURCE & GEAR", "장비 티어는 문이 아니라 속도 차이다", "상위 자원을 아예 막지 않고, 낮은 장비로 캐면 매우 느리게 만들어 위험 감수를 만든다.");
    const ores = [["돌", "철", "미스릴", "아다만티움"]];
    const woods = [["가지", "소나무", "참나무", "흑단"]];
    addText(s, "광석", { left: 104, top: 240, width: 120, height: 36 }, { size: 28, bold: true, color: C.cyan });
    addText(s, "나무", { left: 104, top: 400, width: 120, height: 36 }, { size: 28, bold: true, color: C.green });
    [0,1,2,3].forEach(i => {
      addNode(s, ores[0][i], { left: 250 + i*220, top: 228, width: 150, height: 72 }, C.cyan);
      addNode(s, woods[0][i], { left: 250 + i*220, top: 388, width: 150, height: 72 }, C.green);
      if (i < 3) { arrowText(s, 410 + i*220, 246, C.cyan); arrowText(s, 410 + i*220, 406, C.green); }
    });
    addText(s, "초기 루프: 돌/가지 줍기 → 돌 도끼/곡괭이 제작 → 철 접근 → 다음 장비 제작", { left: 172, top: 552, width: 920, height: 42 }, { size: 24, bold: true, color: C.ember, align: "center" });
    addText(s, "거점에서 멀수록 높은 등급 자원과 비싼 상자가 등장한다. 낮은 등급일수록 더 자주 재생성된다.", { left: 188, top: 610, width: 900, height: 32 }, { size: 18, color: C.mute, align: "center" });
    addFooter(s, 10);
  }

  // 11 building defense
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.night, { left: 0, top: 0, width: W, height: H }, 0.36);
    s.shapes.add({ geometry: "rect", position: { left: 0, top: 0, width: W, height: H }, fill: "#000000", opacity: 0.28, line: { style: "solid", fill: "none", width: 0 } });
    addTitle(s, "BASE DEFENSE", "고정 건축 지점으로 MVP를 단단하게 만든다", "처음부터 모든 건축 지점이 보이며, 플레이어는 자원을 들고 E를 길게 눌러 건설한다.");
    const items = [
      ["벽", "외곽 / 중간 / 불꽃 근처 순서로 방어선 형성\n체력 보유, 막히면 적이 파괴", 95, C.green],
      ["포탑", "자동 공격. 적은 포탑을 최우선 타깃으로 보지 않지만\n맞거나 길이 막히면 파괴 대상이 된다.", 470, C.cyan],
      ["함정", "사라지지 않는 지속 영역. 피해는 낮고\n감속/기절 같은 제어에 특화.", 845, C.ember],
    ];
    items.forEach(([t,b,x,col]) => {
      addText(s, t, { left: x, top: 258, width: 250, height: 42 }, { size: 36, bold: true, color: col, align: "center" });
      addText(s, b, { left: x - 25, top: 326, width: 300, height: 128 }, { size: 18, align: "center" });
      addText(s, "업그레이드 가능", { left: x - 12, top: 492, width: 274, height: 32 }, { size: 22, bold: true, color: col, align: "center" });
    });
    addText(s, "부서지면 잔해가 남고 같은 자리에서 수리/재건축한다. 건설 중 피격은 취소가 아니라 건설 속도 감소로 처리한다.", { left: 112, top: 604, width: 1050, height: 34 }, { size: 20, color: C.ink, align: "center" });
    addFooter(s, 11);
  }

  // 12 enemies and boss
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.boss, { left: 670, top: 0, width: 610, height: H }, 0.62);
    addTitle(s, "ENEMY & BOSS AI", "적은 매일 쌓이고, 보스는 결승선이 아니다", "3일차 밤에는 일반 몬스터가 계속 나오며 보스가 3분 동안 불꽃 심장을 향해 접근한다.");
    const rows = [
      ["1일차", "스켈레톤", "느린 기본 근접. 수로 압박한다."],
      ["2일차", "+ 얼음 좀비", "체력이 높고 구조물 파괴 압력이 강하다."],
      ["3일차", "+ 얼음늑대", "빠르게 플레이어를 물고 늘어진다."],
      ["최종", "보스", "불꽃 심장으로 이동. 플레이어가 위협하면 추격 후 복귀."],
    ];
    rows.forEach((r, i) => {
      const y = 245 + i * 78;
      addText(s, r[0], { left: 86, top: y, width: 120, height: 30 }, { size: 21, bold: true, color: i === 3 ? C.red : C.ember });
      addText(s, r[1], { left: 230, top: y, width: 190, height: 30 }, { size: 23, bold: true });
      addText(s, r[2], { left: 430, top: y + 1, width: 300, height: 42 }, { size: 17, color: C.mute });
    });
    addText(s, "타깃 규칙: 불꽃에 가까워지는 경로를 우선. 막히거나 더 가까워질 수 없으면 주변 구조물을 파괴한다.", { left: 82, top: 594, width: 690, height: 48 }, { size: 19, color: C.ink });
    addFooter(s, 12);
  }

  // 13 class diagram
  {
    const s = p.slides.add(); addBg(s);
    addTitle(s, "CLASS DIAGRAM", "코드 일관화 기준 역할 분리", "Controller는 순서와 큐만 지휘한다. 행동은 각 오브젝트의 전용 컴포넌트가 처리한다.");
    const classes = [
      ["GameManager\n날짜 / 실패 / 클리어\nInstance Model 소유", 70, 210, 250, 94, C.green],
      ["GameFlowController\n낮/밤/보스 순서\n웨이브 시작 알림", 405, 210, 260, 94, C.ember],
      ["GameDataManager\nJSON 로드\nStatic Data 조회", 740, 210, 260, 94, C.cyan],
      ["Player\nMovement / Interaction\nCombat / Inventory", 70, 430, 250, 116, C.cyan],
      ["FlameHeart\nHealth / WarmthAura\nBeam / Upgrade", 405, 430, 260, 116, C.ember],
      ["WorldObject\nGatherable / Chest\nBuildSite / Structure", 740, 430, 260, 116, C.green],
      ["Enemy\nTargetSelector\nMovement / Attack / Drop", 1030, 430, 190, 116, C.red],
    ];
    classes.forEach(([t,x,y,w,h,col]) => addNode(s, t, { left: x, top: y, width: w, height: h }, col));
    line(s, 320, 258, 405, 258, C.line, 2); arrowText(s, 365, 240, C.line);
    line(s, 665, 258, 740, 258, C.line, 2); arrowText(s, 700, 240, C.line);
    line(s, 535, 304, 535, 430, C.line, 2); arrowText(s, 520, 354, C.line);
    line(s, 870, 304, 870, 430, C.line, 2); arrowText(s, 855, 354, C.line);
    line(s, 665, 488, 740, 488, C.line, 2); arrowText(s, 700, 470, C.line);
    line(s, 1000, 488, 1030, 488, C.line, 2); arrowText(s, 1002, 470, C.line);
    addText(s, "참조 5개 이상 필요한 Controller는 분리 대상. UI는 Prefab Instantiate. 멤버변수는 _prefix, Unity 참조는 [SerializeField] private.", { left: 100, top: 610, width: 1080, height: 44 }, { size: 18, color: C.mute, align: "center" });
    addFooter(s, 13);
  }

  // 14 data architecture
  {
    const s = p.slides.add(); addBg(s);
    addTitle(s, "DATA ARCHITECTURE", "데이터는 기획 변경을 견디게 만든다", "수치 조정은 코드 수정이 아니라 Excel → JSON → GameDataManager 로드로 처리한다.");
    const cols = [
      ["Static Data", "OO_Resource\nOO_Recipe\nOO_Relic\nOO_Enemy\nOO_FlameUpgrade\nOO_Building", "GameDataBase 상속\n런타임 중 변하지 않음", 90, C.cyan],
      ["Instance Model", "PlayerModel\nFlameHeartModel\nInventoryModel\nRunProgressModel", "GameManager가 소유\n저장/진행 상태", 470, C.green],
      ["Runtime Component", "Movement\nInteraction\nTargetSelector\nBuildProgress\nWarmthAura", "각 배우가 직접 보유\nController 직접 참조 금지", 850, C.ember],
    ];
    cols.forEach(([h, list, desc, x, col]) => {
      addText(s, h, { left: x, top: 232, width: 300, height: 38 }, { size: 27, bold: true, color: col, align: "center" });
      addText(s, list, { left: x, top: 300, width: 300, height: 160 }, { size: 22, bold: true, align: "center" });
      addText(s, desc, { left: x, top: 504, width: 300, height: 76 }, { size: 18, color: C.mute, align: "center" });
    });
    addText(s, "핵심 원칙: Controller는 '언제'만 말하고, Component가 '어떻게'를 실행한다.", { left: 144, top: 620, width: 990, height: 34 }, { size: 24, bold: true, color: C.ink, align: "center" });
    addFooter(s, 14);
  }

  // 15 roadmap
  {
    const s = p.slides.add(); addBg(s);
    addTitle(s, "GDR 6/16 - 7/3", "개발 로드맵", "MVP 15분 생존 루프를 먼저 고정하고, 이후 30분 이상 확장 가능한 구조를 붙인다.");
    const phases = [
      ["6/16-6/18", "기초 루프", "이동/시점, 낮밤 타이머, 자원 채집, 기본 제작"],
      ["6/19-6/22", "성장", "상자/코인/유물, 장비 슬롯, 허기/스태미나"],
      ["6/23-6/26", "거점 방어", "불꽃 심장, 고정 건축 지점, 벽/포탑/함정"],
      ["6/27-6/30", "전투", "3종 몬스터, 타깃팅, 보스 접근/처치 클리어"],
      ["7/1-7/3", "폴리싱", "밸런스, UI 피드백, 버그 수정, 15분 플레이 검증"],
    ];
    phases.forEach((p, i) => {
      const x = 82 + i * 232;
      addText(s, p[0], { left: x, top: 248, width: 190, height: 28 }, { size: 18, bold: true, color: i < 2 ? C.cyan : i < 4 ? C.ember : C.green, align: "center" });
      addRect(s, { left: x + 16, top: 294, width: 158, height: 18 }, i < 2 ? C.cyan : i < 4 ? C.ember : C.green, "none", "rounded-sm");
      addText(s, p[1], { left: x, top: 342, width: 190, height: 32 }, { size: 25, bold: true, align: "center" });
      addText(s, p[2], { left: x - 8, top: 394, width: 206, height: 116 }, { size: 16, color: C.mute, align: "center" });
      if (i < 4) arrowText(s, x + 184, 286, C.line);
    });
    addText(s, "확장 예약: 자유 건축, 장기 모드, 희귀 장비, 추위 게이지, 추가 보스는 MVP 구조가 안정된 뒤 분기한다.", { left: 118, top: 602, width: 1040, height: 36 }, { size: 21, color: C.ink, align: "center" });
    addFooter(s, 15);
  }

  // 16 team handoff
  {
    const s = p.slides.add(); addBg(s);
    await addImage(s, imgs.boss, { left: 0, top: 0, width: W, height: H }, 0.3);
    s.shapes.add({ geometry: "rect", position: { left: 0, top: 0, width: W, height: H }, fill: "#000000", opacity: 0.26, line: { style: "solid", fill: "none", width: 0 } });
    addTitle(s, "TEAM HANDOFF", "팀원이 바로 잡아야 할 구현 기준", "이 덱의 목적은 컨셉 설명이 아니라, 첫 구현에서 흔들리지 않을 기준을 주는 것이다.");
    const left = [
      "플레이어 사망 또는 불꽃 체력 0이면 즉시 실패",
      "보스는 3일차 밤에 접근 타이머와 함께 출현",
      "일반 몬스터는 보스 접근 중에도 계속 스폰",
      "불꽃 업그레이드는 플레이어 스탯이 아니라 불꽃 전용",
      "상자 유물은 영구 누적이며 화면에서 확인 가능",
    ];
    const right = [
      "건축은 MVP에서 고정 지점 + E 홀드 게이지",
      "피격 시 건설 취소가 아니라 건설 속도 감소",
      "함정은 저피해, 감속/기절 중심. 보스 효율 감소",
      "장비 티어는 소프트락: 상위 자원 채집 가능하지만 느림",
      "Controller는 순서만 관리하고 직접 참조를 늘리지 않는다",
    ];
    addText(s, left.map(v => `• ${v}`).join("\n"), { left: 92, top: 242, width: 520, height: 280 }, { size: 20, color: C.ink });
    addText(s, right.map(v => `• ${v}`).join("\n"), { left: 690, top: 242, width: 500, height: 280 }, { size: 20, color: C.ink });
    addText(s, "완성 기준: 15분 안에 첫 채집, 첫 제작, 첫 상자, 첫 밤 방어, 첫 불꽃 강화, 보스 접근까지 모두 경험된다.", { left: 120, top: 590, width: 1040, height: 50 }, { size: 24, bold: true, color: C.ember, align: "center" });
    addFooter(s, 16);
  }

  for (const [i, slide] of p.slides.items.entries()) {
    const stem = `slide-${String(i + 1).padStart(2, "0")}`;
    const png = await p.export({ slide, format: "png", scale: 1 });
    await fs.writeFile(path.join(PREVIEW, `${stem}.png`), new Uint8Array(await png.arrayBuffer()));
    const layout = await slide.export({ format: "layout" });
    await fs.writeFile(path.join(LAYOUT, `${stem}.layout.json`), await layout.text(), "utf8");
  }
  const montage = await p.export({ format: "webp", montage: true, scale: 1 });
  await fs.writeFile(path.join(PREVIEW, "deck-montage.webp"), new Uint8Array(await montage.arrayBuffer()));
  const pptx = await PresentationFile.exportPptx(p);
  await pptx.save(OUT);

  const snapshot = await p.inspect({ kind: "slide,textbox,shape,image,chart,table", maxChars: 20000 });
  await fs.writeFile(path.join(QA, "inspect.ndjson"), snapshot.ndjson, "utf8");
  await fs.writeFile(path.join(QA, "source-notes.txt"), [
    "Cinderkeep GDP V1",
    "Facts: user-provided design discussion in Codex thread.",
    "Assets: user-provided/generated low-poly concept images from CK 게임 컨셉 folder.",
    "No external factual claims used.",
  ].join("\n"), "utf8");
  await fs.writeFile(path.join(QA, "visual-qa.txt"), [
    "Rendered all 16 slides to PNG.",
    "Design target: large open layouts, minimal decorative boxes, full-slide diagrams for flowchart and class diagram.",
    "Text fit strategy: short slide copy, large text frames, no dense paragraphs in diagram nodes.",
  ].join("\n"), "utf8");
}

build().catch(err => {
  console.error(err);
  process.exitCode = 1;
});
