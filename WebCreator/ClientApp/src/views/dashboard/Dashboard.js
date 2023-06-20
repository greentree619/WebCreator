import React, { useEffect, useState, createRef } from 'react'

import {
  CAvatar,
  CButton,
  CButtonGroup,
  CCard,
  CCardBody,
  CCardFooter,
  CCardHeader,
  CCol,
  CProgress,
  CRow,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CWidgetStatsF,
  CBadge,
} from '@coreui/react'
import { CChartLine } from '@coreui/react-chartjs'
import { getStyle, hexToRgba } from '@coreui/utils'
import CIcon from '@coreui/icons-react'
import {
  cibCcAmex,
  cibCcApplePay,
  cibCcMastercard,
  cibCcPaypal,
  cibCcStripe,
  cibCcVisa,
  cibGoogle,
  cibFacebook,
  cibLinkedin,
  cifBr,
  cifEs,
  cifFr,
  cifIn,
  cifPl,
  cifUs,
  cibTwitter,
  cilSettings,
  cilCloudDownload,
  cilPeople,
  cilWindow,
  cilUser,
  cilUserFemale,
  cibCanva,
} from '@coreui/icons'
import { Outlet, Link } from 'react-router-dom'

import avatar1 from 'src/assets/images/avatars/1.jpg'
import avatar2 from 'src/assets/images/avatars/2.jpg'
import avatar3 from 'src/assets/images/avatars/3.jpg'
import avatar4 from 'src/assets/images/avatars/4.jpg'
import avatar5 from 'src/assets/images/avatars/5.jpg'
import avatar6 from 'src/assets/images/avatars/6.jpg'

import WidgetsBrand from '../widgets/WidgetsBrand'
import WidgetsDropdown from '../widgets/WidgetsDropdown'
import Truncate from 'react-truncate'
import { useDispatch, useSelector } from 'react-redux'
import { loadFromLocalStorage, saveToLocalStorage, clearLocalStorage } from 'src/utility/common'
import { ReactSession }  from 'react-client-session'

const Dashboard = () => {
  const dispatch = useDispatch()
  const random = (min, max) => Math.floor(Math.random() * (max - min + 1) + min)
  const [projects, setProjects] = useState([])

  const progressExample = [
    { title: 'Visits', value: '29.703 Users', percent: 40, color: 'success' },
    { title: 'Unique', value: '24.093 Users', percent: 20, color: 'info' },
    { title: 'Pageviews', value: '78.706 Views', percent: 60, color: 'warning' },
    { title: 'New Users', value: '22.123 Users', percent: 80, color: 'danger' },
    { title: 'Bounce Rate', value: 'Average Rate', percent: 40.15, color: 'primary' },
  ]

  const progressGroupExample1 = [
    { title: 'Monday', value1: 34, value2: 78 },
    { title: 'Tuesday', value1: 56, value2: 94 },
    { title: 'Wednesday', value1: 12, value2: 67 },
    { title: 'Thursday', value1: 43, value2: 91 },
    { title: 'Friday', value1: 22, value2: 73 },
    { title: 'Saturday', value1: 53, value2: 82 },
    { title: 'Sunday', value1: 9, value2: 69 },
  ]

  const progressGroupExample2 = [
    { title: 'Male', icon: cilUser, value: 53 },
    { title: 'Female', icon: cilUserFemale, value: 43 },
  ]

  const progressGroupExample3 = [
    { title: 'Organic Search', icon: cibGoogle, percent: 56, value: '191,235' },
    { title: 'Facebook', icon: cibFacebook, percent: 15, value: '51,223' },
    { title: 'Twitter', icon: cibTwitter, percent: 11, value: '37,564' },
    { title: 'LinkedIn', icon: cibLinkedin, percent: 8, value: '27,319' },
  ]

  useEffect(() => {
    dispatch({ type: 'set', activeDomainName: '', activeProject: {}, activeDomainId: '' })
    getAllProject()
  }, [])

  async function getAllProject() {
    var allProjects = loadFromLocalStorage('allProjects')
    console.log( allProjects )

    var prevAllProjects = ReactSession.get("allProjects")
    //console.log("prevAllProjects 1", prevAllProjects)
    if(prevAllProjects === undefined || prevAllProjects === "0")
    {
      const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/1/200`)
      const data = await response.json()
      console.log(data.data)
      saveToLocalStorage(data.data, 'allProjects')
      setProjects(data.data)

      ReactSession.set("allProjects", "1")
      //prevAllProjects = ReactSession.get("allProjects")
      //console.log("prevAllProjects 2", prevAllProjects)
    }
    else
    {
      if(allProjects != null && allProjects != undefined && allProjects.length > 0)
      {
        setProjects( allProjects )
      }
    }
  }

  return (
    <CRow>
      <CCol xs={15} sm={6} lg={3} className="my-2">
        <Link to={`/project/add`} state={{ simple_mode: true }}>
          <CWidgetStatsF
            className="lb-3"
            icon={<CIcon width={24} icon={cilWindow} size="xl" />}
            title="Create Website"
            value=""
            color="primary"
          />
        </Link>
      </CCol>
      {projects.map((project) => (
        <CCol key={project.id} xs={15} sm={6} lg={3} my={2} className="my-2">
          <Link to={`/project/add`} state={{ mode: 'VIEW', project: project }}>
            <CWidgetStatsF
              className="lb-3"
              icon={<CIcon width={24} icon={cilWindow} size="xl" />}
              title={project.name}
              value={<><CBadge color="secondary" size="sm">keywords</CBadge>
                        <CBadge color="secondary" size="sm">articles</CBadge>
                        <CBadge color="secondary" size="sm">publish</CBadge></>}
              color="primary"
            />
          </Link>
        </CCol>
      ))}
    </CRow>
  )
}

export default Dashboard
