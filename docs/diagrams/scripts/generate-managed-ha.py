from pathlib import Path
import html
import xml.etree.ElementTree as ET

folder = Path(__file__).resolve().parents[1]
source = folder / 'auction-azure-architecture.drawio'
tree = ET.parse(source)
root = tree.getroot()
for page in list(root):
    if page.get('id') == 'managed-ha':
        root.remove(page)
page = ET.SubElement(root, 'diagram', id='managed-ha', name='04 — Managed HA configuration')
model = ET.SubElement(page, 'mxGraphModel', dx='1400', dy='980', grid='1', gridSize='10', page='1', pageWidth='1400', pageHeight='980')
cells = ET.SubElement(model, 'root')
ET.SubElement(cells, 'mxCell', id='0')
ET.SubElement(cells, 'mxCell', id='1', parent='0')
svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="1400" height="980" viewBox="0 0 1400 980">',
       '<defs><marker id="arrow" markerWidth="8" markerHeight="8" refX="7" refY="4" orient="auto"><path d="M0 0 L8 4 L0 8 Z" fill="#52677e"/></marker></defs>',
       '<rect width="1400" height="980" fill="#f6f8fc"/>']

def box(key, x, y, w, h, title, lines=(), color='#ffffff', border='#b5c5d8', title_only=False):
    label = '<b>' + html.escape(title) + '</b>' + ''.join('<br>' + html.escape(t) for t in lines)
    cell = ET.SubElement(cells, 'mxCell', id=key, value=label, vertex='1', parent='1',
        style=f'rounded=1;whiteSpace=wrap;html=1;fillColor={color};strokeColor={border};fontColor=#142b46;fontSize=16;spacing=12;verticalAlign={"top" if title_only else "middle"};')
    ET.SubElement(cell, 'mxGeometry', x=str(x), y=str(y), width=str(w), height=str(h), **{'as':'geometry'})
    svg.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="12" fill="{color}" stroke="{border}" stroke-width="1.5"/>')
    count = len(lines) + 1
    top = y + 28 if title_only else y + (h - count * 24) / 2 + 18
    for i, text in enumerate([title, *lines]):
        weight = '600' if i == 0 else '400'
        size = 18 if i == 0 else 15
        svg.append(f'<text x="{x+w/2}" y="{top+i*24}" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="{size}" font-weight="{weight}" fill="#142b46">{html.escape(text)}</text>')

def edge(key, points, label='', dashed=False):
    style = 'endArrow=block;html=1;strokeColor=#52677e;strokeWidth=2;rounded=0;' + ('dashed=1;' if dashed else '')
    c = ET.SubElement(cells, 'mxCell', id=key, value=label, edge='1', parent='1', style=style)
    g = ET.SubElement(c, 'mxGeometry', relative='1', **{'as':'geometry'})
    ET.SubElement(g, 'mxPoint', x=str(points[0][0]), y=str(points[0][1]), **{'as':'sourcePoint'})
    ET.SubElement(g, 'mxPoint', x=str(points[-1][0]), y=str(points[-1][1]), **{'as':'targetPoint'})
    if len(points)>2:
        a=ET.SubElement(g,'Array',**{'as':'points'})
        for x,y in points[1:-1]: ET.SubElement(a,'mxPoint',x=str(x),y=str(y))
    dash=' stroke-dasharray="7 5"' if dashed else ''
    svg.append('<polyline points="'+' '.join(f'{x},{y}' for x,y in points)+f'" fill="none" stroke="#52677e" stroke-width="2"{dash} marker-end="url(#arrow)"/>')
    if label:
        x,y=points[len(points)//2]
        svg.append(f'<text x="{x+8}" y="{y-10}" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#52677e">{html.escape(label)}</text>')

box('title',30,20,1340,60,'Auction platform | Managed deployment configuration',
    ['Repository configuration • Single region • Providers and CDN require provisioning'],color='#e8f1fc',border='#e8f1fc')
box('aks',390,350,600,310,'AKS | replicas spread across nodes and zones',
    ['13 Deployments · 28 starting replicas · 12 HPAs'],color='#e8f1fc',border='#6c9ed1',title_only=True)
box('users',540,105,320,65,'Users / browser', ['HTTPS + authenticated live updates'])
box('edge',540,215,320,95,'Optional Azure Front Door', ['/assets/* cached', 'HTML, API, auth, hubs: cache disabled'],color='#e3f5fa')
box('realtime',1030,215,330,95,'Azure SignalR', ['Shared delivery across notification pods', 'Browser connects directly after negotiation'],color='#e3f5fa')
box('ops',40,215,310,120,'Delivery and operations', ['GitHub Actions → ACR → AKS', 'Key Vault → External Secrets', 'Azure Monitor / Log Analytics'])
box('blob',40,500,300,110,'Azure Blob Storage', ['Private files and signed downloads', 'No global public-media cache'],color='#f0edf9')
box('web',425,435,210,65,'Web / NGINX', ['Same-origin /api and /hubs'])
box('gateway',725,435,210,65,'Gateway / YARP', ['HTTP routes + hub negotiation'])
box('apps',425,540,510,95,'11 backend services', ['Bidding: PostgreSQL advisory locks', 'Notification: Azure SignalR · Auction: browse DTO cache'])
box('postgres',40,760,300,115,'PostgreSQL Flexible Server', ['Zone-redundant HA', '10 logical databases', 'Authoritative bids and transactions'],color='#e9f3ea')
box('redis',380,760,280,115,'Azure Managed Redis', ['HA cache / deduplication / locks', 'Live browse cache: five-second TTL', 'Cache does not validate bids'],color='#e9f3ea')
box('mq',700,760,300,115,'Managed RabbitMQ', ['AMQPS 5671 · three-node quorum', 'Provider plan + migration required', 'Delayed delivery needs separate validation'],color='#fff2dd')
box('search',1040,760,320,115,'Managed Elasticsearch', ['HTTPS 443 · multiple zones', 'Replicated indexes + snapshots', 'Provider plan + migration required'],color='#fff2dd')
edge('users-edge',[(700,170),(700,215)])
edge('users-hub',[(860,137),(1195,137),(1195,215)],'Direct live connection')
edge('edge-web',[(620,310),(620,330),(410,330),(410,468),(425,468)])
edge('web-gateway',[(635,468),(725,468)])
edge('gateway-apps',[(830,500),(830,540)])
edge('apps-hub',[(935,585),(1195,585),(1195,310)])
edge('apps-blob',[(425,585),(340,585)])
edge('ops-aks',[(350,280),(375,280),(375,450),(390,450)],dashed=True)
edge('apps-db',[(480,635),(480,695),(190,695),(190,760)])
edge('apps-redis',[(580,635),(580,720),(520,720),(520,760)])
edge('apps-mq',[(780,635),(780,695),(850,695),(850,760)])
edge('apps-es',[(900,635),(900,720),(1200,720),(1200,760)])
box('footnote',40,905,1320,50,'Configuration is not proof of capacity or failover.',
    ['Validate load, bid correctness, provider failover and recovery before production cutover.'],color='#f6f8fc',border='#f6f8fc')
svg.append('</svg>')
(folder/'auction-managed-ha.svg').write_text('\n'.join(svg)+'\n')
ET.indent(tree, space='  ')
tree.write(source, encoding='utf-8', xml_declaration=True)
print('Updated managed HA SVG and editable page 04.')
