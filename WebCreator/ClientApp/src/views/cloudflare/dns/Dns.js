import React, { useEffect, Component } from 'react'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CButton,
  CAlert,
  CPagination,
  CPaginationItem,
  CRow,
  CCol,
  CContainer,
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { Outlet, Link } from 'react-router-dom'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import PropTypes from 'prop-types'
import { ToastContainer, toast } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'
import { alertConfirmOption } from 'src/utility/common'
import { confirmAlert } from 'react-confirm-alert'
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css

class DnsBase extends Component {
  static displayName = DnsBase.name

  constructor(props) {
    super(props)
    this.state = {
      listData: [],
      loading: true,
      alarmVisible: false,
      zoneName: '',
      alertMsg: '',
      alertColor: 'success',
      curPage: 1,
      totalPage: 1,
      zoneInfo:
        this.props.location.state == null
          ? { zoneId: '', zoneName: '' }
          : this.props.location.state,
    }
  }

  componentDidMount() {
    this.populateData(1)
  }

  gotoPrevPage() {
    this.populateData(this.state.curPage - 1)
  }

  gotoNextPage() {
    this.populateData(this.state.curPage + 1)
  }

  deleteDnsConfirm = (dnsName) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this dnse.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => this.deleteDns(dnsName)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  async deleteDns(dnsName) {
    const requestOptions = {
      method: 'GET'
    }
    fetch(`${process.env.REACT_APP_SERVER_URL}dns/deleteDns/${dnsName}`, requestOptions)
      .then((response) => {
        if (response.status === 200) {
          response.json().then(data => {
            if( data.result )
            {
              console.log('success:', data.result)
              toast.success('DNS \"' + dnsName + '\" was deleted successfully.', alertConfirmOption);
            }
            else
            {
              console.log('failed:', data.result)
              toast.error(`Failed to delete this DNS ${dnsName}.`, alertConfirmOption);
            }
          })
        }
      })
      .catch((err) => console.log(err))
  }

  renderProjectsTable(state) {
    let pageButtonCount = 3
    let pagination = <p></p>

    if (this.state.totalPage > 1) {
      let prevButton = (
        <CPaginationItem onClick={() => this.gotoPrevPage()}>Previous</CPaginationItem>
      )
      if (state.curPage <= 1) prevButton = <CPaginationItem disabled>Previous</CPaginationItem>

      let nextButton = <CPaginationItem onClick={() => this.gotoNextPage()}>Next</CPaginationItem>
      if (state.curPage >= state.totalPage)
        nextButton = <CPaginationItem disabled>Next</CPaginationItem>

      var pageNoAry = []
      var startNo = state.curPage - pageButtonCount
      var endNo = state.curPage + pageButtonCount
      if (startNo < 1) {
        startNo = 1
        endNo =
          pageButtonCount * 2 + 1 > state.totalPage ? state.totalPage : pageButtonCount * 2 + 1
      } else if (endNo > state.totalPage) {
        endNo = state.totalPage
        startNo = endNo - pageButtonCount * 2 > 1 ? endNo - pageButtonCount * 2 : 1
      }

      for (var i = startNo; i <= endNo; i++) {
        if (i < 1 || i > state.totalPage) continue
        pageNoAry.push(i)
      }

      const paginationItems = pageNoAry.map((number) => (
        <CPaginationItem
          key={number}
          onClick={() => this.populateData(number)}
          active={number == state.curPage}
        >
          {number}
        </CPaginationItem>
      ))

      pagination = (
        <CPagination align="center" aria-label="Page navigation example">
          {prevButton}
          {paginationItems}
          {nextButton}
        </CPagination>
      )
    }

    return (
      <>
        <CAlert
          color={state.alertColor}
          dismissible
          visible={state.alarmVisible}
          onClose={() => this.setState({ alarmVisible: false })}
        >
          {state.alertMsg}
        </CAlert>
        <ToastContainer
          position="top-right"
          autoClose={5000}
          hideProgressBar={false}
          newestOnTop={false}
          closeOnClick
          rtl={false}
          pauseOnFocusLoss
          draggable
          pauseOnHover
          theme="colored"
        />
        <table className="table">
          <thead>
            <tr>
              <th className='text-center'>Id</th>
              <th className='text-center'>Domain</th>
              <th className='text-center'>Content</th>
              <th className='text-center'>Type</th>
              <th className='text-center'>TTL</th>
              <th className='text-center'>Action</th>
            </tr>
          </thead>
          <tbody>
            {state.listData.map((dns) => (
              <tr key={dns.id}>
                <td className='text-center'>{dns.id}</td>
                <td>{dns.name}</td>
                <td>{dns.content}</td>
                <td className='text-center'>{dns.type}</td>
                <td className='text-center'>{dns.ttl}</td>
                <td className='text-center'>
                  <CButton type="button" onClick={() => this.deleteDnsConfirm(dns.name)}>
                    Delete
                  </CButton>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {pagination}
      </>
    )
  }

  render() {
    let contents = this.state.loading ? (
      <p>
        <em>
          {this.state.zoneInfo.zoneId.length > 0
            ? 'Loading...'
            : 'Please select one zone from zone list.'}
        </em>
      </p>
    ) : (
      this.renderProjectsTable(this.state)
    )
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <CContainer>
            <CRow>
              <CCol className="align-self-start">All DNS For {this.state.zoneInfo.zoneName}</CCol>
              <CCol className="align-self-end" xs="auto">
                <CButton
                  type="button"
                  onClick={() => this.props.navigate(-1)}
                >
                  Back
                </CButton>
              </CCol>
            </CRow>
          </CContainer>
        </CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateData(pageNo) {
    if (this.state.zoneInfo != null) {
      const response = await fetch(
        `${process.env.REACT_APP_SERVER_URL}dns/${this.state.zoneInfo.zoneId}/` + pageNo + '/200',
      )
      const data = await response.json()
      this.setState({
        listData: data.result,
        loading: false,
        alarmVisible: false,
        curPage: data.curPage,
        totalPage: data.total,
      })
    }
  }
}

DnsBase.propTypes = {
  location: PropTypes.any,
  navigate: PropTypes.any,
}

const Dns = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const navigate = useNavigate()

  //console.log(location.state)
  if (location.state == null && location.search.length > 0) {
    location.state = { 
      zoneId: new URLSearchParams(location.search).get('zoneId'),
      zoneName: new URLSearchParams(location.search).get('domainName'),
   }
  }

  useEffect(() => {
    dispatch({ type: 'set', activeTab: 'cloudflare_dns' })
  }, [])

  return <DnsBase location={location} navigate ={navigate} {...props} />
}
export default Dns
