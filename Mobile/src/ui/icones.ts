/* ==========================================================================
   Ícones — o mesmo conjunto Tabler Icons usado no web (classes "ti ti-...")
   --------------------------------------------------------------------------
   O web carrega a fonte de ícones Tabler por CDN; aqui é a versão React
   Native do MESMO pacote, desenhada em SVG. Mesmo desenho de ícone nas duas
   plataformas.

   POR QUE IMPORTAR UM POR UM: o pacote tem mais de 6.000 ícones. Importar do
   índice ("@tabler/icons-react-native") colocaria todos no APK. Importando
   pelo caminho de cada arquivo, entra no app só o que é usado.

   ALTERNATIVA REJEITADA: @expo/vector-icons, que já vem no Expo. Não tem o
   conjunto Tabler, e trocar de família de ícones afastaria o app do visual
   do site sem nenhum ganho.
   ========================================================================== */
import IconAffiliate from '@tabler/icons-react-native/IconAffiliate';
import IconAlertTriangle from '@tabler/icons-react-native/IconAlertTriangle';
import IconArrowLeft from '@tabler/icons-react-native/IconArrowLeft';
import IconArrowRight from '@tabler/icons-react-native/IconArrowRight';
import IconAt from '@tabler/icons-react-native/IconAt';
import IconAward from '@tabler/icons-react-native/IconAward';
import IconBell from '@tabler/icons-react-native/IconBell';
import IconBook from '@tabler/icons-react-native/IconBook';
import IconBook2 from '@tabler/icons-react-native/IconBook2';
import IconBookmark from '@tabler/icons-react-native/IconBookmark';
import IconBuilding from '@tabler/icons-react-native/IconBuilding';
import IconCalendar from '@tabler/icons-react-native/IconCalendar';
import IconCertificate from '@tabler/icons-react-native/IconCertificate';
import IconChartBar from '@tabler/icons-react-native/IconChartBar';
import IconCheck from '@tabler/icons-react-native/IconCheck';
import IconChecks from '@tabler/icons-react-native/IconChecks';
import IconChevronDown from '@tabler/icons-react-native/IconChevronDown';
import IconChevronRight from '@tabler/icons-react-native/IconChevronRight';
import IconCircleCheck from '@tabler/icons-react-native/IconCircleCheck';
import IconCircleX from '@tabler/icons-react-native/IconCircleX';
import IconClipboardList from '@tabler/icons-react-native/IconClipboardList';
import IconClock from '@tabler/icons-react-native/IconClock';
import IconCloudOff from '@tabler/icons-react-native/IconCloudOff';
import IconCode from '@tabler/icons-react-native/IconCode';
import IconCopy from '@tabler/icons-react-native/IconCopy';
import IconCrown from '@tabler/icons-react-native/IconCrown';
import IconDatabase from '@tabler/icons-react-native/IconDatabase';
import IconDeviceFloppy from '@tabler/icons-react-native/IconDeviceFloppy';
import IconEdit from '@tabler/icons-react-native/IconEdit';
import IconEye from '@tabler/icons-react-native/IconEye';
import IconEyeOff from '@tabler/icons-react-native/IconEyeOff';
import IconFileText from '@tabler/icons-react-native/IconFileText';
import IconFlame from '@tabler/icons-react-native/IconFlame';
import IconHeadset from '@tabler/icons-react-native/IconHeadset';
import IconHelpCircle from '@tabler/icons-react-native/IconHelpCircle';
import IconHelpSquare from '@tabler/icons-react-native/IconHelpSquare';
import IconHistory from '@tabler/icons-react-native/IconHistory';
import IconHome from '@tabler/icons-react-native/IconHome';
import IconInfoCircle from '@tabler/icons-react-native/IconInfoCircle';
import IconKey from '@tabler/icons-react-native/IconKey';
import IconLayoutDashboard from '@tabler/icons-react-native/IconLayoutDashboard';
import IconList from '@tabler/icons-react-native/IconList';
import IconLock from '@tabler/icons-react-native/IconLock';
import IconLogout from '@tabler/icons-react-native/IconLogout';
import IconMail from '@tabler/icons-react-native/IconMail';
import IconMapPin from '@tabler/icons-react-native/IconMapPin';
import IconMedal from '@tabler/icons-react-native/IconMedal';
import IconMessage2 from '@tabler/icons-react-native/IconMessage2';
import IconMessageCircle from '@tabler/icons-react-native/IconMessageCircle';
import IconMessageCircleCheck from '@tabler/icons-react-native/IconMessageCircleCheck';
import IconMessageReply from '@tabler/icons-react-native/IconMessageReply';
import IconPhone from '@tabler/icons-react-native/IconPhone';
import IconPlayerPlay from '@tabler/icons-react-native/IconPlayerPlay';
import IconPlus from '@tabler/icons-react-native/IconPlus';
import IconRefresh from '@tabler/icons-react-native/IconRefresh';
import IconSchool from '@tabler/icons-react-native/IconSchool';
import IconSearch from '@tabler/icons-react-native/IconSearch';
import IconSend from '@tabler/icons-react-native/IconSend';
import IconShieldCheck from '@tabler/icons-react-native/IconShieldCheck';
import IconStar from '@tabler/icons-react-native/IconStar';
import IconStarFilled from '@tabler/icons-react-native/IconStarFilled';
import IconTargetArrow from '@tabler/icons-react-native/IconTargetArrow';
import IconTool from '@tabler/icons-react-native/IconTool';
import IconTrash from '@tabler/icons-react-native/IconTrash';
import IconTrophy from '@tabler/icons-react-native/IconTrophy';
import IconUpload from '@tabler/icons-react-native/IconUpload';
import IconUser from '@tabler/icons-react-native/IconUser';
import IconUserBolt from '@tabler/icons-react-native/IconUserBolt';
import IconUserOff from '@tabler/icons-react-native/IconUserOff';
import IconUsers from '@tabler/icons-react-native/IconUsers';
import IconX from '@tabler/icons-react-native/IconX';

/** Chave = nome usado no web sem o prefixo "ti-". */
export const ICONES = {
  affiliate: IconAffiliate,
  'alert-triangle': IconAlertTriangle,
  'arrow-left': IconArrowLeft,
  'arrow-right': IconArrowRight,
  at: IconAt,
  award: IconAward,
  bell: IconBell,
  book: IconBook,
  'book-2': IconBook2,
  bookmark: IconBookmark,
  building: IconBuilding,
  calendar: IconCalendar,
  certificate: IconCertificate,
  'chart-bar': IconChartBar,
  check: IconCheck,
  checks: IconChecks,
  'chevron-down': IconChevronDown,
  'chevron-right': IconChevronRight,
  'circle-check': IconCircleCheck,
  'circle-x': IconCircleX,
  'clipboard-list': IconClipboardList,
  clock: IconClock,
  'cloud-off': IconCloudOff,
  code: IconCode,
  copy: IconCopy,
  crown: IconCrown,
  database: IconDatabase,
  'device-floppy': IconDeviceFloppy,
  edit: IconEdit,
  eye: IconEye,
  'eye-off': IconEyeOff,
  'file-text': IconFileText,
  flame: IconFlame,
  headset: IconHeadset,
  'help-circle': IconHelpCircle,
  'help-square': IconHelpSquare,
  history: IconHistory,
  home: IconHome,
  'info-circle': IconInfoCircle,
  key: IconKey,
  'layout-dashboard': IconLayoutDashboard,
  list: IconList,
  lock: IconLock,
  logout: IconLogout,
  mail: IconMail,
  'map-pin': IconMapPin,
  medal: IconMedal,
  'message-2': IconMessage2,
  'message-circle': IconMessageCircle,
  'message-circle-check': IconMessageCircleCheck,
  'message-reply': IconMessageReply,
  phone: IconPhone,
  'player-play': IconPlayerPlay,
  plus: IconPlus,
  refresh: IconRefresh,
  school: IconSchool,
  search: IconSearch,
  send: IconSend,
  'shield-check': IconShieldCheck,
  star: IconStar,
  'star-filled': IconStarFilled,
  'target-arrow': IconTargetArrow,
  tool: IconTool,
  trash: IconTrash,
  trophy: IconTrophy,
  upload: IconUpload,
  user: IconUser,
  'user-bolt': IconUserBolt,
  'user-off': IconUserOff,
  users: IconUsers,
  x: IconX
} as const;

export type NomeIcone = keyof typeof ICONES;
