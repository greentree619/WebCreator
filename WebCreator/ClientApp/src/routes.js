import React from 'react'
import { useLocation } from 'react-router-dom'

const Dashboard = React.lazy(() => import('./views/dashboard/Dashboard'))
const Colors = React.lazy(() => import('./views/theme/colors/Colors'))
const Typography = React.lazy(() => import('./views/theme/typography/Typography'))
const ArticleThemeUpload = React.lazy(() => import('./views/theme/article/Article'))
const AddProject = React.lazy(() => import('./views/project/add/Add'))
const Keyword = React.lazy(() => import('./views/project/keyword/Keyword'))
const ListProject = React.lazy(() => import('./views/project/list/List'))
const AllArticles = React.lazy(() => import('./views/article/list/List'))
const NewArticle = React.lazy(() => import('./views/article/add/Add'))
const LogView = React.lazy(() => import('./views/log/View'))
const ArticleView = React.lazy(() => import('./views/article/view/View'))
const ApprovalArticle = React.lazy(() => import('./views/article/approval/Approval'))
const AFSetting = React.lazy(() => import('./views/article/setting/Setting'))
const OpenAISetting = React.lazy(() => import('./views/openai/setting/Setting'))
const AllZoneList = React.lazy(() => import('./views/cloudflare/zone/Zone'))
const AllDnsList = React.lazy(() => import('./views/cloudflare/dns/Dns'))
const BuildSync = React.lazy(() => import('./views/build/build/BuildSync'))
const AFSchedule = React.lazy(() => import('./views/schedule/view/View'))
const Sync = React.lazy(() => import('./views/sync/view/View'))
const History = React.lazy(() => import('./views/history/view/View'))
const S3Bucket = React.lazy(() => import('./views/amazone/bucket/Bucket'))
const S3Contents = React.lazy(() => import('./views/amazone/contents/Contents'))
const EC2Ipaddress = React.lazy(() => import('./views/amazone/ec2/EC2Ipaddress'))
const SyncWithDomain = () => {
      const location = useLocation()
      //console.log(new URLSearchParams(location.search).get('url'));
      const link = new URLSearchParams(location.search).get('url');
      window.open(
        link,
        '_blank' // <- This is what makes it open in a new window.
      );
      console.log('openlink');
  }

// Base
const Accordion = React.lazy(() => import('./views/base/accordion/Accordion'))
const Breadcrumbs = React.lazy(() => import('./views/base/breadcrumbs/Breadcrumbs'))
const Cards = React.lazy(() => import('./views/base/cards/Cards'))
const Carousels = React.lazy(() => import('./views/base/carousels/Carousels'))
const Collapses = React.lazy(() => import('./views/base/collapses/Collapses'))
const ListGroups = React.lazy(() => import('./views/base/list-groups/ListGroups'))
const Navs = React.lazy(() => import('./views/base/navs/Navs'))
const Paginations = React.lazy(() => import('./views/base/paginations/Paginations'))
const Placeholders = React.lazy(() => import('./views/base/placeholders/Placeholders'))
const Popovers = React.lazy(() => import('./views/base/popovers/Popovers'))
const Progress = React.lazy(() => import('./views/base/progress/Progress'))
const Spinners = React.lazy(() => import('./views/base/spinners/Spinners'))
const Tables = React.lazy(() => import('./views/base/tables/Tables'))
const Tooltips = React.lazy(() => import('./views/base/tooltips/Tooltips'))

// Buttons
const Buttons = React.lazy(() => import('./views/buttons/buttons/Buttons'))
const ButtonGroups = React.lazy(() => import('./views/buttons/button-groups/ButtonGroups'))
const Dropdowns = React.lazy(() => import('./views/buttons/dropdowns/Dropdowns'))

//Forms
const ChecksRadios = React.lazy(() => import('./views/forms/checks-radios/ChecksRadios'))
const FloatingLabels = React.lazy(() => import('./views/forms/floating-labels/FloatingLabels'))
const FormControl = React.lazy(() => import('./views/forms/form-control/FormControl'))
const InputGroup = React.lazy(() => import('./views/forms/input-group/InputGroup'))
const Layout = React.lazy(() => import('./views/forms/layout/Layout'))
const Range = React.lazy(() => import('./views/forms/range/Range'))
const Select = React.lazy(() => import('./views/forms/select/Select'))
const Validation = React.lazy(() => import('./views/forms/validation/Validation'))

const Charts = React.lazy(() => import('./views/charts/Charts'))

// Icons
const CoreUIIcons = React.lazy(() => import('./views/icons/coreui-icons/CoreUIIcons'))
const Flags = React.lazy(() => import('./views/icons/flags/Flags'))
const Brands = React.lazy(() => import('./views/icons/brands/Brands'))

// Notifications
const Alerts = React.lazy(() => import('./views/notifications/alerts/Alerts'))
const Badges = React.lazy(() => import('./views/notifications/badges/Badges'))
const Modals = React.lazy(() => import('./views/notifications/modals/Modals'))
const Toasts = React.lazy(() => import('./views/notifications/toasts/Toasts'))

const Widgets = React.lazy(() => import('./views/widgets/Widgets'))

const routes = [
  { path: '/', exact: true, name: 'Home' },
  { path: '/dashboard', name: 'Dashboard', element: Dashboard },
  { path: '/theme', name: 'Theme', element: Colors, exact: true },
  { path: '/theme/colors', name: 'Colors', element: Colors },
  { path: '/theme/typography', name: 'Typography', element: Typography },
  { path: '/theme/article', name: 'Article Theme Upload', element: ArticleThemeUpload },
  { path: '/project/add', name: 'AddProject', element: AddProject },
  { path: '/project/list', name: 'All Websites', element: ListProject },
  { path: '/project/keyword', name: 'Keyword', element: Keyword },
  { path: '/article/list', name: 'All Articles', element: AllArticles },
  { path: '/article/view', name: 'Article View', element: ArticleView },
  { path: '/article/add', name: 'Create New Article', element: NewArticle },
  { path: '/article/approval', name: 'Approval Article', element: ApprovalArticle },
  { path: '/log/view', name: 'Log View', element: LogView },
  { path: '/article/setting', name: 'Article Forge Setting', element: AFSetting },
  { path: '/openai/setting', name: 'OpenAI API Setting', element: OpenAISetting },
  { path: '/cloudflare/zone', name: 'Zone List View', element: AllZoneList },
  { path: '/cloudflare/dns', name: 'Dns List View', element: AllDnsList },
  { path: '/amazone/bucket', name: 'Amazone S3 Bucket Management', element: S3Bucket },
  { path: '/amazone/contents', name: 'Amazone S3 Bucket Contents', element: S3Contents },  
  { path: '/amazone/ec2', name: 'Amazone EC2 IP Address Management', element: EC2Ipaddress },
  { path: '/build/build', name: 'Build & SYNC', element: BuildSync },
  { path: '/schedule/view', name: 'Article Forge Schedule', element: AFSchedule},
  { path: '/sync/view', name: 'Sync with domain', element: Sync},
  { path: '/history/view', name: 'History View', element: History},
  { path: '/base', name: 'Base', element: Cards, exact: true },
  { path: '/base/accordion', name: 'Accordion', element: Accordion },
  { path: '/base/breadcrumbs', name: 'Breadcrumbs', element: Breadcrumbs },
  { path: '/base/cards', name: 'Cards', element: Cards },
  { path: '/base/carousels', name: 'Carousel', element: Carousels },
  { path: '/base/collapses', name: 'Collapse', element: Collapses },
  { path: '/base/list-groups', name: 'List Groups', element: ListGroups },
  { path: '/base/navs', name: 'Navs', element: Navs },
  { path: '/base/paginations', name: 'Paginations', element: Paginations },
  { path: '/base/placeholders', name: 'Placeholders', element: Placeholders },
  { path: '/base/popovers', name: 'Popovers', element: Popovers },
  { path: '/base/progress', name: 'Progress', element: Progress },
  { path: '/base/spinners', name: 'Spinners', element: Spinners },
  { path: '/base/tables', name: 'Tables', element: Tables },
  { path: '/base/tooltips', name: 'Tooltips', element: Tooltips },
  { path: '/buttons', name: 'Buttons', element: Buttons, exact: true },
  { path: '/buttons/buttons', name: 'Buttons', element: Buttons },
  { path: '/buttons/dropdowns', name: 'Dropdowns', element: Dropdowns },
  { path: '/buttons/button-groups', name: 'Button Groups', element: ButtonGroups },
  { path: '/charts', name: 'Charts', element: Charts },
  { path: '/forms', name: 'Forms', element: FormControl, exact: true },
  { path: '/forms/form-control', name: 'Form Control', element: FormControl },
  { path: '/forms/select', name: 'Select', element: Select },
  { path: '/forms/checks-radios', name: 'Checks & Radios', element: ChecksRadios },
  { path: '/forms/range', name: 'Range', element: Range },
  { path: '/forms/input-group', name: 'Input Group', element: InputGroup },
  { path: '/forms/floating-labels', name: 'Floating Labels', element: FloatingLabels },
  { path: '/forms/layout', name: 'Layout', element: Layout },
  { path: '/forms/validation', name: 'Validation', element: Validation },
  { path: '/icons', exact: true, name: 'Icons', element: CoreUIIcons },
  { path: '/icons/coreui-icons', name: 'CoreUI Icons', element: CoreUIIcons },
  { path: '/icons/flags', name: 'Flags', element: Flags },
  { path: '/icons/brands', name: 'Brands', element: Brands },
  { path: '/notifications', name: 'Notifications', element: Alerts, exact: true },
  { path: '/notifications/alerts', name: 'Alerts', element: Alerts },
  { path: '/notifications/badges', name: 'Badges', element: Badges },
  { path: '/notifications/modals', name: 'Modals', element: Modals },
  { path: '/notifications/toasts', name: 'Toasts', element: Toasts },
  { path: '/widgets', name: 'Widgets', element: Widgets },
  { path: '/openlink', element: SyncWithDomain},
]

export default routes
