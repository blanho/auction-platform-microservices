# Auction architecture diagrams

Open `auction-azure-architecture.drawio` in [draw.io](https://app.diagrams.net/)
using **File → Open From → Device**, or open it with the draw.io desktop app.
The file has four editable pages. Pages 1–3 preserve the 24 October baseline;
page 4 shows the new managed-provider configuration:

1. **Azure deployment** — application resource group, VNet and subnet boundaries,
   AKS workloads, managed data services, secrets, monitoring, and delivery.
2. **Runtime flows** — bid entry and gRPC relationships, sale coordination,
   search projections, real-time notifications, and file storage.
3. **Kubernetes details** — all 13 application Deployments, configured replicas,
   autoscaling ranges and targets, declared Service port mappings, data ownership,
   backend connection paths, and rollout controls. This page uses the rendered
   production Kustomize overlay, including production patches.
4. **Managed HA configuration** — SignalR scale-out, browse caching, replica
   spreading, managed data providers, and optional Front Door CDN delivery.

Quick previews:

- [Managed HA configuration — current option](auction-managed-ha.svg)
- [Azure deployment](auction-azure-deployment.svg)
- [Runtime flows](auction-runtime-flows.svg)
- [Kubernetes details](auction-kubernetes-details.svg)

Shapes, labels and connectors are editable. Nested groups preserve the resource
boundaries. Double-click into a group to edit its contents; select the outer
group to move a section together. Azure icons are embedded SVG assets, so the
diagram does not depend on remote image URLs.

The SVG files are previews for sharing and documentation.
After changing the draw.io file, use **File → Export as → SVG** to refresh
the previews, selecting the relevant page and a white background.

Regenerate page 4 and its matching native SVG with
`python3 docs/diagrams/scripts/generate-managed-ha.py`. Editing page 4 manually
and then running that script will replace those manual edits; change the script
when maintaining this generated view. Pages 1–3 are preserved by the script.

The baseline diagrams reflect repository configuration reviewed on 2026-10-24. They
describe one configured Azure region. See the [architecture notes](../cloud-deployment-architecture.md)
for source files and deployment findings.

Azure icons come from Microsoft's [Azure Architecture Center icon collection](https://learn.microsoft.com/en-us/azure/architecture/icons/),
pack V24. Microsoft permits these icons in architecture diagrams, training
materials and documentation; other rights are reserved. The icons retain their
original appearance.
