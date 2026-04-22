import { useEffect, useMemo, useRef, useState, type FormEvent, type PointerEvent } from 'react'
import './App.css'

type InputMode = 'url' | 'html'
type Algorithm = 'bfs' | 'dfs'
type ResultMode = 'all' | 'top'
type Theme = 'light' | 'dark'
type ViewMode = 'idle' | 'tree' | 'traversal' // <-- TAMBAHAN STATE BARU

interface DomNode {
  nodeId: number
  tagName: string
  label: string
  id: string
  classes: string[]
  attributes: Record<string, string>
  depth: number
  childCount: number
  path: string
  children: DomNode[]
  hiddenCount?: number
  isSummary?: boolean
}

interface AnalysisStats {
  nodeCount: number
  maxDepth: number
  visitedCount: number
  totalMatches: number
  displayedMatches: number
  scrapeMilliseconds: number
  parseMilliseconds: number
  searchMilliseconds: number
}

interface TraversalLogItem {
  order: number
  nodeId: number
  label: string
  tagName: string
  depth: number
  path: string
  isSolution: boolean
  isAffected: boolean
}

interface ResultNode {
  rank: number
  nodeId: number
  label: string
  tagName: string
  id: string
  classes: string[]
  attributes: Record<string, string>
  depth: number
  path: string
}

interface AffectedGroup {
  level: number
  nodeIds: number[]
}

interface SelectorQuery {
  tagName: string
  id: string
  classes: string[]
  attributeName: string
  attributeValue: string
  relationToPrevious: string
}

interface AnalyzeResponse {
  dom: DomNode
  stats: AnalysisStats
  traversalLog: TraversalLogItem[]
  results: ResultNode[]
  affectedGroups: AffectedGroup[]
  parsedSelector: SelectorQuery[]
  logFileName: string
}

interface AnalyzeError {
  error: string
}

interface LayoutNode extends DomNode {
  x: number
  y: number
}

interface LayoutEdge {
  from: number
  to: number
}

interface ViewPan {
  x: number
  y: number
}

interface DragState {
  pointerId: number
  startX: number
  startY: number
  panX: number
  panY: number
}

interface LayoutConfig {
  nodeWidth: number
  nodeHeight: number
  horizontalGap: number
  verticalGap: number
  compact: boolean
}

interface TreeLayout {
  nodes: LayoutNode[]
  edges: LayoutEdge[]
  nodeMap: Map<number, LayoutNode>
  width: number
  height: number
  config: LayoutConfig
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''
const minZoom = 0.35
const maxZoom = 1.8
const defaultPan = { x: 42, y: 34 }

const sampleHtml = `<!doctype html>
<html>
  <head>
    <title>DOM Traversal Sample</title>
  </head>
  <body>
    <header id="hero" class="section dark">
      <nav class="menu">
        <a class="menu-item active" href="/">Home</a>
        <a class="menu-item" href="/docs">Docs</a>
      </nav>
    </header>
    <main>
      <section class="card featured">
        <h1 id="title">Traversal BFS DFS</h1>
        <p class="lead">Visualisasi penelusuran CSS selector.</p>
      </section>
      <section class="card">
        <button class="cta primary">Mulai</button>
        <button class="cta">Detail</button>
      </section>
    </main>
  </body>
</html>`

const iconPaths = {
  graph: 'M4 6.5a2.5 2.5 0 1 1 4.9.7l4.2 2.4a2.5 2.5 0 1 1 0 4.8l-4.2 2.4A2.5 2.5 0 1 1 8 15.3l4.2-2.4a2.8 2.8 0 0 1 0-1.8L8 8.7A2.5 2.5 0 0 1 4 6.5Z',
  search: 'm15.5 15.5 4 4M10.8 17a6.2 6.2 0 1 1 0-12.4 6.2 6.2 0 0 1 0 12.4Z',
  link: 'M9.5 7.5 11 6a4 4 0 0 1 5.7 5.7l-1.8 1.8a4 4 0 0 1-5.7 0M14.5 16.5 13 18a4 4 0 1 1-5.7-5.7l1.8-1.8a4 4 0 0 1 5.7 0',
  code: 'm8 9-4 3 4 3M16 9l4 3-4 3M14 5l-4 14',
  play: 'M8 5v14l11-7-11-7Z',
  pause: 'M7 5h4v14H7ZM13 5h4v14h-4Z',
  target: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18ZM12 17a5 5 0 1 0 0-10 5 5 0 0 0 0 10ZM12 13a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z',
  list: 'M8 6h12M8 12h12M8 18h12M4 6h.01M4 12h.01M4 18h.01',
  sun: 'M12 4V2M12 22v-2M4 12H2M22 12h-2M5 5l-1.4-1.4M20.4 20.4 19 19M19 5l1.4-1.4M3.6 20.4 5 19M16 12a4 4 0 1 1-8 0 4 4 0 0 1 8 0Z',
  moon: 'M20 14.5A8.5 8.5 0 0 1 9.5 4a7 7 0 1 0 10.5 10.5Z',
  zoomIn: 'M11 5v12M5 11h12M15.5 15.5l4 4M10.8 17a6.2 6.2 0 1 1 0-12.4 6.2 6.2 0 0 1 0 12.4Z',
  zoomOut: 'M5 11h12M15.5 15.5l4 4M10.8 17a6.2 6.2 0 1 1 0-12.4 6.2 6.2 0 0 1 0 12.4Z',
  reset: 'M4 4v6h6M20 20v-6h-6M5.2 14A7 7 0 0 0 17 17.2M18.8 10A7 7 0 0 0 7 6.8',
  fullscreen: 'M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5',
  fullscreenExit: 'M9 4v5H4M15 4v5h5M9 20v-5H4M15 20v-5h5',
}

function Icon({ name }: { name: keyof typeof iconPaths }) {
  return (
    <svg className="icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d={iconPaths[name]} />
    </svg>
  )
}

function getInitialTheme(): Theme {
  const saved = window.localStorage.getItem('dom-theme')
  if (saved === 'light' || saved === 'dark') {
    return saved
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function getLayoutConfig(nodeCount: number): LayoutConfig {
  if (nodeCount > 700) {
    return {
      nodeWidth: 116,
      nodeHeight: 38,
      horizontalGap: 12,
      verticalGap: 72,
      compact: true,
    }
  }

  if (nodeCount > 220) {
    return {
      nodeWidth: 136,
      nodeHeight: 44,
      horizontalGap: 18,
      verticalGap: 82,
      compact: true,
    }
  }

  return {
    nodeWidth: 172,
    nodeHeight: 58,
    horizontalGap: 42,
    verticalGap: 104,
    compact: false,
  }
}

function getInitialVisibleDepth(nodeCount: number, maxDepth: number) {
  if (nodeCount > 700) {
    return Math.min(maxDepth, 5)
  }

  if (nodeCount > 220) {
    return Math.min(maxDepth, 6)
  }

  return maxDepth
}

function countDescendants(node: DomNode): number {
  return node.children.reduce((total, child) => total + 1 + countDescendants(child), 0)
}

function createSummaryNode(parent: DomNode): DomNode {
  const hiddenCount = countDescendants(parent)

  return {
    nodeId: -parent.nodeId,
    tagName: 'summary',
    label: `+ ${hiddenCount} node`,
    id: '',
    classes: [],
    attributes: {},
    depth: parent.depth + 1,
    childCount: 0,
    path: `${parent.path} > hidden-descendants`,
    children: [],
    hiddenCount,
    isSummary: true,
  }
}

function limitTreeDepth(node: DomNode, visibleDepth: number): DomNode {
  if (node.depth >= visibleDepth && node.children.length > 0) {
    return {
      ...node,
      children: [createSummaryNode(node)],
    }
  }

  return {
    ...node,
    children: node.children.map((child) => limitTreeDepth(child, visibleDepth)),
  }
}

function buildLayout(root: DomNode, config: LayoutConfig): TreeLayout {
  const nodes: LayoutNode[] = []
  const edges: LayoutEdge[] = []
  let leafCursor = 0

  function walk(node: DomNode): LayoutNode {
    const childLayouts = node.children.map(walk)
    let x: number

    if (childLayouts.length === 0) {
      x = leafCursor * (config.nodeWidth + config.horizontalGap)
      leafCursor += 1
    } else {
      x = (childLayouts[0].x + childLayouts[childLayouts.length - 1].x) / 2
    }

    const layoutNode: LayoutNode = {
      ...node,
      x,
      y: node.depth * config.verticalGap,
    }

    nodes.push(layoutNode)
    childLayouts.forEach((child) => edges.push({ from: layoutNode.nodeId, to: child.nodeId }))
    return layoutNode
  }

  walk(root)

  const width = Math.max(...nodes.map((node) => node.x + config.nodeWidth), config.nodeWidth) + 64
  const height = Math.max(...nodes.map((node) => node.y + config.nodeHeight), config.nodeHeight) + 64
  const nodeMap = new Map(nodes.map((node) => [node.nodeId, node]))

  return { nodes, edges, nodeMap, width, height, config }
}

function flattenTree(root: DomNode) {
  const map = new Map<number, DomNode>()

  function visit(node: DomNode) {
    map.set(node.nodeId, node)
    node.children.forEach(visit)
  }

  visit(root)
  return map
}

function formatMs(value: number) {
  if (value < 1) {
    return `${value.toFixed(2)} ms`
  }

  return `${value.toFixed(1)} ms`
}

function clamp(value: number, minimum: number, maximum: number) {
  return Math.min(maximum, Math.max(minimum, value))
}

function getFittedView(stage: HTMLDivElement | null, layout: TreeLayout, preferredZoom = 0.95) {
  if (!stage) {
    return { zoom: preferredZoom, pan: defaultPan }
  }

  const rect = stage.getBoundingClientRect()
  const padding = 72
  const fitZoom = Math.min(preferredZoom, (rect.width - padding) / layout.width, (rect.height - padding) / layout.height)
  const nextZoom = clamp(fitZoom, minZoom, maxZoom)

  return {
    zoom: nextZoom,
    pan: {
      x: Math.max(24, (rect.width - layout.width * nextZoom) / 2),
      y: 32,
    },
  }
}

function describeSelector(query: SelectorQuery) {
  const relation = query.relationToPrevious === 'None' ? '' : `${query.relationToPrevious} `
  const tag = query.tagName || '*'
  const id = query.id ? `#${query.id}` : ''
  const classes = query.classes.map((className) => `.${className}`).join('')
  const attribute = query.attributeName ? `[${query.attributeName}=${query.attributeValue}]` : ''
  return `${relation}${tag}${id}${classes}${attribute}`
}

function App() {
  const [theme, setTheme] = useState<Theme>(getInitialTheme)
  const [inputMode, setInputMode] = useState<InputMode>('html')
  const [url, setUrl] = useState('https://example.com')
  const [html, setHtml] = useState(sampleHtml)
  const [selector, setSelector] = useState('.card .cta')
  const [algorithm, setAlgorithm] = useState<Algorithm>('bfs')
  const [resultMode, setResultMode] = useState<ResultMode>('all')
  const [limit, setLimit] = useState(10)
  
  const [viewMode, setViewMode] = useState<ViewMode>('idle')
  const [isParsing, setIsParsing] = useState(false)
  const [isSearching, setIsSearching] = useState(false)
  
  const [response, setResponse] = useState<AnalyzeResponse | null>(null)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [selectedNodeId, setSelectedNodeId] = useState<number | null>(null)
  const [playbackStep, setPlaybackStep] = useState(0)
  const [isPlaying, setIsPlaying] = useState(false)
  const [zoom, setZoom] = useState(0.9)
  const [pan, setPan] = useState<ViewPan>(defaultPan)
  const [dragState, setDragState] = useState<DragState | null>(null)
  const [visibleDepth, setVisibleDepth] = useState(6)
  const [isTreeFullscreen, setIsTreeFullscreen] = useState(false)
  const visualAreaRef = useRef<HTMLElement | null>(null)
  const treeStageRef = useRef<HTMLDivElement | null>(null)
  const layoutRef = useRef<TreeLayout | null>(null)
  const zoomRef = useRef(zoom)
  const panRef = useRef(pan)

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    window.localStorage.setItem('dom-theme', theme)
  }, [theme])

  useEffect(() => {
    function handleFullscreenChange() {
      setIsTreeFullscreen(document.fullscreenElement === visualAreaRef.current)
    }

    document.addEventListener('fullscreenchange', handleFullscreenChange)
    return () => document.removeEventListener('fullscreenchange', handleFullscreenChange)
  }, [])

  useEffect(() => {
    zoomRef.current = zoom
  }, [zoom])

  useEffect(() => {
    panRef.current = pan
  }, [pan])

  useEffect(() => {
    if (!isPlaying || !response) {
      return
    }

    const intervalId = window.setInterval(() => {
      setPlaybackStep((current) => {
        if (current >= response.traversalLog.length) {
          window.clearInterval(intervalId)
          setIsPlaying(false)
          return current
        }

        return current + 1
      })
    }, 90)

    return () => window.clearInterval(intervalId)
  }, [isPlaying, response, viewMode])

  const displayDom = useMemo(() => (response ? limitTreeDepth(response.dom, visibleDepth) : null), [response, visibleDepth])

  const layout = useMemo(() => {
    if (!response || !displayDom) {
      return null
    }

    return buildLayout(displayDom, getLayoutConfig(response.stats.nodeCount))
  }, [displayDom, response])

  useEffect(() => {
    layoutRef.current = layout
  }, [layout])

  useEffect(() => {
    const stageElement = treeStageRef.current
    if (!stageElement) return

    function handleNativeWheel(event: globalThis.WheelEvent) {
      const currentLayout = layoutRef.current
      if (!currentLayout) {
        return
      }

      event.preventDefault()
      event.stopPropagation()

      const rect = stageElement!.getBoundingClientRect()
      const originX = event.clientX - rect.left
      const originY = event.clientY - rect.top
      const previousZoom = zoomRef.current
      const nextZoom = clamp(previousZoom - event.deltaY * 0.0012, minZoom, maxZoom)
      const currentPan = panRef.current

      setZoom(nextZoom)
      setPan({
        x: originX - ((originX - currentPan.x) / previousZoom) * nextZoom,
        y: originY - ((originY - currentPan.y) / previousZoom) * nextZoom,
      })
    }

    stageElement.addEventListener('wheel', handleNativeWheel, { passive: false })
    return () => stageElement.removeEventListener('wheel', handleNativeWheel)
  }, [])

  const nodeLookup = useMemo(() => (response ? flattenTree(response.dom) : new Map<number, DomNode>()), [response])

  const solutionNodeIds = useMemo(() => {
    return new Set(response?.traversalLog.filter((item) => item.isSolution).map((item) => item.nodeId) ?? [])
  }, [response])

  const affectedNodeIds = useMemo(() => {
    return new Set(response?.affectedGroups.flatMap((group) => group.nodeIds) ?? [])
  }, [response])

  const activeTraversalIds = useMemo(() => {
    if (!response || viewMode !== 'traversal') return new Set<number>()
    return new Set(response.traversalLog.slice(0, playbackStep).map((item) => item.nodeId))
  }, [playbackStep, response, viewMode])

  const currentTraversalNodeId = (viewMode === 'traversal' && response?.traversalLog[Math.max(0, playbackStep - 1)]?.nodeId) || null
  const selectedNode = selectedNodeId ? nodeLookup.get(selectedNodeId) ?? null : null

  async function handleParse() {
    setIsParsing(true)
    setError('')
    setResponse(null)
    setSelectedNodeId(null)
    setViewMode('idle')

    try {
      const res = await fetch(`${apiBaseUrl}/api/analyze`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          inputMode, url, html,
          algorithm: 'bfs',
          selector: '*', // Dummy selector agar backend meloloskan validasi
          resultMode: 'all',
          limit: 10
        }),
      })

      if (!res.ok) {
        const payload = (await res.json()) as AnalyzeError
        throw new Error(payload.error || 'Gagal mem-parse HTML.')
      }

      const payload = (await res.json()) as AnalyzeResponse
      const nextVisibleDepth = getInitialVisibleDepth(payload.stats.nodeCount, payload.stats.maxDepth)
      const nextLayout = buildLayout(
        limitTreeDepth(payload.dom, nextVisibleDepth),
        getLayoutConfig(payload.stats.nodeCount),
      )
      const nextView = getFittedView(treeStageRef.current, nextLayout)

      setResponse(payload)
      setViewMode('tree')
      setVisibleDepth(nextVisibleDepth)
      setZoom(nextView.zoom)
      setPan(nextView.pan)
      setDragState(null)
      setSelectedNodeId(payload.dom.nodeId)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Gagal mem-parse HTML.')
    } finally {
      setIsParsing(false)
    }
  }

  async function handleTraverse() {
    setIsSearching(true)
    setError('')

    try {
      const res = await fetch(`${apiBaseUrl}/api/analyze`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          inputMode, url, html,
          algorithm, selector, resultMode, limit,
        }),
      })

      if (!res.ok) {
        const payload = (await res.json()) as AnalyzeError
        throw new Error(payload.error || 'Analisis gagal dijalankan.')
      }

      const payload = (await res.json()) as AnalyzeResponse
      setResponse(payload)
      setViewMode('traversal')
      setPlaybackStep(0)
      setIsPlaying(true)
      setSelectedNodeId(payload.results[0]?.nodeId ?? payload.dom.nodeId)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Analisis gagal dijalankan.')
    } finally {
      setIsSearching(false)
    }
  }

  function handlePlayPause() {
    if (!response || viewMode !== 'traversal') return
    if (isPlaying) {
      setIsPlaying(false)
      return
    }
    setPlaybackStep((current) => (current >= response.traversalLog.length ? 0 : current))
    setIsPlaying(true)
  }

  function handleTreePointerDown(event: PointerEvent<HTMLDivElement>) {
    if (!layout || event.button !== 0) {
      return
    }

    if (event.target instanceof Element && event.target.closest('button, input, textarea, select, a')) {
      return
    }

    event.currentTarget.setPointerCapture(event.pointerId)
    setDragState({
      pointerId: event.pointerId,
      startX: event.clientX,
      startY: event.clientY,
      panX: pan.x,
      panY: pan.y,
    })
  }

  function handleTreePointerMove(event: PointerEvent<HTMLDivElement>) {
    if (!dragState || event.pointerId !== dragState.pointerId) {
      return
    }

    setPan({
      x: dragState.panX + event.clientX - dragState.startX,
      y: dragState.panY + event.clientY - dragState.startY,
    })
  }

  function finishTreeDrag(event: PointerEvent<HTMLDivElement>) {
    if (!dragState || event.pointerId !== dragState.pointerId) {
      return
    }

    if (event.currentTarget.hasPointerCapture(event.pointerId)) {
      event.currentTarget.releasePointerCapture(event.pointerId)
    }

    setDragState(null)
  }

  function resetTreeView() {
    if (layout) {
      const nextView = getFittedView(treeStageRef.current, layout)
      setZoom(nextView.zoom)
      setPan(nextView.pan)
    } else {
      setZoom(0.9)
      setPan(defaultPan)
    }

    setDragState(null)
  }

  async function toggleTreeFullscreen() {
    const visualArea = visualAreaRef.current
    if (!visualArea) {
      return
    }

    try {
      if (document.fullscreenElement === visualArea) {
        await document.exitFullscreen()
      } else {
        await visualArea.requestFullscreen()
      }
    } catch {
      setIsTreeFullscreen((current) => !current)
    }
  }

  function handleVisibleDepthChange(nextDepth: number) {
    if (!response) {
      return
    }

    const clampedDepth = clamp(nextDepth, 1, response.stats.maxDepth)
    const nextLayout = buildLayout(
      limitTreeDepth(response.dom, clampedDepth),
      getLayoutConfig(response.stats.nodeCount),
    )
    const nextView = getFittedView(treeStageRef.current, nextLayout)

    setVisibleDepth(clampedDepth)
    setZoom(nextView.zoom)
    setPan(nextView.pan)
    setDragState(null)
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">
          <span className="brand-mark" style={{ background: 'transparent', border: 'none' }}>
            <img 
              src="/logo.png" 
              alt="DOM logo" 
              style={{ width: '100%', height: '100%', objectFit: 'contain' }} 
            />
          </span>
          <div>
            <h1>DOM Traversal</h1>
            <span>IF2211 Strategi Algoritma</span>
          </div>
        </div>

        <button
          className="theme-toggle"
          type="button"
          onClick={() => setTheme((current) => (current === 'dark' ? 'light' : 'dark'))}
          aria-label="Ganti tema"
        >
          <Icon name={theme === 'dark' ? 'sun' : 'moon'} />
          {theme === 'dark' ? 'Light Mode' : 'Dark Mode'}
        </button>
      </header>

      <main className="workspace">
        <aside className="sidebar">
          <div className="control-panel">
            <section className="control-section">
              <h2>1. Input HTML</h2>
              <div className="segmented" role="group" aria-label="Mode input">
                <button className={inputMode === 'url' ? 'active' : ''} type="button" onClick={() => setInputMode('url')}>
                  <Icon name="link" /> URL
                </button>
                <button className={inputMode === 'html' ? 'active' : ''} type="button" onClick={() => setInputMode('html')}>
                  <Icon name="code" /> HTML
                </button>
              </div>

              {inputMode === 'url' ? (
                <label className="field">
                  <span>URL website</span>
                  <input value={url} onChange={(event) => setUrl(event.target.value)} />
                </label>
              ) : (
                <label className="field">
                  <span>Teks HTML</span>
                  <textarea value={html} onChange={(event) => setHtml(event.target.value)} />
                </label>
              )}
              
              <button 
                className="primary-action" 
                disabled={isParsing || isSearching} 
                type="button" 
                onClick={handleParse}
                style={{ marginTop: '4px' }}
              >
                <Icon name="code" />
                {isParsing ? 'Mem-parse...' : 'Parse HTML'}
              </button>
            </section>

            <section className={`control-section ${viewMode === 'idle' ? 'disabled-section' : ''}`}>
              <h2>2. Traversal CSS</h2>
              <label className="field">
                <span>CSS selector</span>
                <input value={selector} onChange={(event) => setSelector(event.target.value)} disabled={viewMode === 'idle'} />
              </label>

              <div className="segmented" role="group" aria-label="Algoritma traversal">
                <button className={algorithm === 'bfs' ? 'active' : ''} type="button" onClick={() => setAlgorithm('bfs')} disabled={viewMode === 'idle'}>
                  BFS
                </button>
                <button className={algorithm === 'dfs' ? 'active' : ''} type="button" onClick={() => setAlgorithm('dfs')} disabled={viewMode === 'idle'}>
                  DFS
                </button>
              </div>

              <div className="result-options">
                <label className="radio-row">
                  <input checked={resultMode === 'all'} name="result-mode" type="radio" onChange={() => setResultMode('all')} disabled={viewMode === 'idle'} />
                  Semua kemunculan
                </label>
                <label className="radio-row">
                  <input checked={resultMode === 'top'} name="result-mode" type="radio" onChange={() => setResultMode('top')} disabled={viewMode === 'idle'} />
                  Top
                  <input className="number-input" disabled={resultMode !== 'top' || viewMode === 'idle'} min={0} type="number" value={limit} onChange={(event) => setLimit(Number(event.target.value))} />
                </label>
              </div>

              <button 
                className="primary-action" 
                disabled={viewMode === 'idle' || isParsing || isSearching} 
                type="button" 
                onClick={handleTraverse}
                style={{ marginTop: '4px' }}
              >
                <Icon name="search" />
                {isSearching ? 'Mencari...' : 'Jalankan Traversal'}
              </button>
            </section>

            {error ? <p className="error-message">{error}</p> : null}
          </div>
        </aside>

        <section className={isTreeFullscreen ? 'visual-area fullscreen-active' : 'visual-area'} ref={visualAreaRef}>
          <div className="visual-toolbar">
            <div>
              <h2>Visualisasi DOM Tree</h2>
              <span>
                {response
                  ? `${response.stats.nodeCount} node, kedalaman maksimum ${response.stats.maxDepth}, depth tampil ${visibleDepth}`
                  : 'Belum ada hasil analisis'}
              </span>
            </div>

            <div className="toolbar-tools">
              <div className="depth-controls">
                <span>Depth</span>
                <input disabled={!response} max={response?.stats.maxDepth ?? 1} min={response && response.stats.maxDepth > 0 ? 1 : 0} step={1} type="range" value={visibleDepth} onChange={(event) => handleVisibleDepthChange(Number(event.target.value))} />
                <strong>{visibleDepth}</strong>
              </div>

              <div className="playback">
                <button className="icon-button" disabled={!response || viewMode !== 'traversal'} type="button" onClick={handlePlayPause}>
                  <Icon name={isPlaying ? 'pause' : 'play'} />
                </button>
                <input disabled={!response || viewMode !== 'traversal'} max={response?.traversalLog.length ?? 0} min={0} type="range" value={playbackStep} onChange={(event) => setPlaybackStep(Number(event.target.value))} />
                <span>{playbackStep}/{response?.traversalLog.length ?? 0}</span>
              </div>

              <div className="zoom-controls">
                <button className="icon-button" disabled={!response} title="Zoom out" type="button" onClick={() => setZoom((current) => clamp(current - 0.1, minZoom, maxZoom))}><Icon name="zoomOut" /></button>
                <input disabled={!response} max={maxZoom} min={minZoom} step={0.05} type="range" value={zoom} onChange={(event) => setZoom(Number(event.target.value))} />
                <span>{Math.round(zoom * 100)}%</span>
                <button className="icon-button" disabled={!response} title="Zoom in" type="button" onClick={() => setZoom((current) => clamp(current + 0.1, minZoom, maxZoom))}><Icon name="zoomIn" /></button>
                <button className="icon-button" disabled={!response} title="Reset view" type="button" onClick={resetTreeView}><Icon name="reset" /></button>
                <button className="icon-button" title={isTreeFullscreen ? 'Exit fullscreen' : 'Fullscreen'} type="button" onClick={toggleTreeFullscreen}><Icon name={isTreeFullscreen ? 'fullscreenExit' : 'fullscreen'} /></button>
              </div>
            </div>
          </div>

          <div className={dragState ? 'tree-stage dragging' : 'tree-stage'} onPointerCancel={finishTreeDrag} onPointerDown={handleTreePointerDown} onPointerMove={handleTreePointerMove} onPointerUp={finishTreeDrag} ref={treeStageRef}>
            {layout ? (
              <div className={layout.config.compact ? 'tree-transform compact' : 'tree-transform'} style={{ transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})` }}>
                <div className="tree-canvas" style={{ width: layout.width, height: layout.height }}>
                  <svg className="tree-edges" width={layout.width} height={layout.height}>
                    {layout.edges.map((edge) => {
                      const from = layout.nodeMap.get(edge.from)
                      const to = layout.nodeMap.get(edge.to)
                      if (!from || !to) return null
                      
                      const isEdgeActive = viewMode === 'traversal' && activeTraversalIds.has(to.nodeId)

                      return (
                        <path
                          className={isEdgeActive ? 'edge active' : 'edge'}
                          d={`M ${from.x + layout.config.nodeWidth / 2} ${from.y + layout.config.nodeHeight} C ${from.x + layout.config.nodeWidth / 2} ${from.y + layout.config.nodeHeight + 24}, ${to.x + layout.config.nodeWidth / 2} ${to.y - 24}, ${to.x + layout.config.nodeWidth / 2} ${to.y}`}
                          key={`${edge.from}-${edge.to}`}
                        />
                      )
                    })}
                  </svg>

                  {layout.nodes.map((node) => {
                    const isVisited = viewMode === 'traversal' && activeTraversalIds.has(node.nodeId)
                    const isSolution = viewMode === 'traversal' && solutionNodeIds.has(node.nodeId)
                    const isAffected = viewMode === 'traversal' && affectedNodeIds.has(node.nodeId)
                    const isCurrent = viewMode === 'traversal' && currentTraversalNodeId === node.nodeId
                    const isSelected = selectedNodeId === node.nodeId

                    return (
                      <button
                        className={[
                          'tree-node',
                          layout.config.compact ? 'compact' : '',
                          node.isSummary ? 'summary' : '',
                          isVisited ? 'visited' : '',
                          isAffected ? 'affected' : '',
                          isSolution ? 'solution' : '',
                          isSelected ? 'selected' : '',
                          isCurrent ? 'current' : '',
                        ].filter(Boolean).join(' ')}
                        disabled={node.isSummary}
                        key={node.nodeId}
                        style={{ left: node.x, top: node.y, width: layout.config.nodeWidth, height: layout.config.nodeHeight }}
                        title={node.path}
                        type="button"
                        onClick={() => setSelectedNodeId(node.nodeId)}
                      >
                        <span className="node-label">{node.label}</span>
                        <span className="node-meta">{node.isSummary ? `${node.hiddenCount} hidden` : `d${node.depth} / ${node.childCount} child`}</span>
                      </button>
                    )
                  })}
                </div>
              </div>
            ) : (
              <div className="empty-state">
                <Icon name="graph" />
                <h2>Masukkan HTML di sebelah kiri</h2>
              </div>
            )}
          </div>
        </section>

        <aside className="inspector">
          <section className="stats-grid" aria-label="Statistik traversal">
            <div><span>Visited</span><strong>{viewMode === 'traversal' ? (response?.stats.visitedCount ?? 0) : '-'}</strong></div>
            <div><span>Match</span><strong>{viewMode === 'traversal' ? (response?.stats.totalMatches ?? 0) : '-'}</strong></div>
            <div><span>Ditampilkan</span><strong>{viewMode === 'traversal' ? (response?.stats.displayedMatches ?? 0) : '-'}</strong></div>
            <div><span>Search</span><strong>{viewMode === 'traversal' && response ? formatMs(response.stats.searchMilliseconds) : '- ms'}</strong></div>
          </section>

          <section className="detail-panel">
            <div className="panel-heading"><Icon name="target" /><h2>Node Detail</h2></div>
            {selectedNode ? (
              <div className="node-detail">
                <strong>{selectedNode.label}</strong>
                <span>Tag: {selectedNode.tagName}</span>
                <span>Depth: {selectedNode.depth}</span>
                <span>Children: {selectedNode.childCount}</span>
                {selectedNode.id ? <span>ID: {selectedNode.id}</span> : null}
                {selectedNode.classes.length > 0 ? <span>Class: {selectedNode.classes.join(', ')}</span> : null}
                <code>{selectedNode.path}</code>
              </div>
            ) : <p className="muted">Pilih node pada tree atau log.</p>}
          </section>

          <section className="detail-panel">
            <div className="panel-heading"><Icon name="search" /><h2>Selector</h2></div>
            {viewMode === 'traversal' && response?.parsedSelector.length ? (
              <>
                <div className="selector-list">
                  {response.parsedSelector.map((query, index) => (
                    <span className="selector-chip" key={`${query.relationToPrevious}-${query.tagName}-${index}`}>{describeSelector(query)}</span>
                  ))}
                </div>
                <p className="log-file">Log: {response.logFileName}</p>
              </>
            ) : <p className="muted">Lakukan traversal untuk melihat selector.</p>}
          </section>

          <section className="detail-panel results-panel">
            <div className="panel-heading"><Icon name="target" /><h2>Hasil</h2></div>
            <div className="scroll-list">
              {viewMode === 'traversal' && response?.results.length ? (
                response.results.map((result) => (
                  <button className={selectedNodeId === result.nodeId ? 'list-row selected' : 'list-row'} key={`${result.rank}-${result.nodeId}`} type="button" onClick={() => setSelectedNodeId(result.nodeId)}>
                    <strong>#{result.rank}</strong><span>{result.label}</span><small>d{result.depth}</small>
                  </button>
                ))
              ) : <p className="muted">{viewMode === 'tree' ? 'Lakukan traversal untuk melihat hasil.' : 'Belum ada hasil.'}</p>}
            </div>
          </section>

          <section className="detail-panel log-panel">
            <div className="panel-heading"><Icon name="list" /><h2>Traversal Log</h2></div>
            <div className="scroll-list">
              {viewMode === 'traversal' && response?.traversalLog.length ? (
                response.traversalLog.map((item) => (
                  <button
                    className={[
                      'list-row',
                      item.isAffected ? 'affected' : '',
                      item.isSolution ? 'solution' : '',
                      selectedNodeId === item.nodeId ? 'selected' : '',
                    ]
                      .filter(Boolean)
                      .join(' ')}
                    key={`${item.order}-${item.nodeId}`}
                    type="button"
                    onClick={() => {
                      setSelectedNodeId(item.nodeId)
                      setPlaybackStep(item.order)
                    }}
                  >
                    <strong>{item.order}</strong>
                    <span>{item.label}</span>
                    <small>d{item.depth}</small>
                  </button>
                ))
              ) : (
                <p className="muted">Log akan muncul setelah pencarian.</p>
              )}
            </div>
          </section>
        </aside>
      </main>
    </div>
  )
}

export default App
