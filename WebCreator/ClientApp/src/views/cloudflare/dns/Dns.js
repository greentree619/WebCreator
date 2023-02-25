import React, { useEffect, Component } from 'react'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CButton,
  CAlert,
  CPagination,
  CPaginationItem,
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { Outlet, Link } from 'react-router-dom'
import { useLocation } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import PropTypes from 'prop-types'

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
        <table className="table">
          <thead>
            <tr>
              <th>Id</th>
              <th>Domain</th>
              <th>Content</th>
              <th>Type</th>
              <th>TTL</th>
            </tr>
          </thead>
          <tbody>
            {state.listData.map((dns) => (
              <tr key={dns.id}>
                <td>{dns.id}</td>
                <td>{dns.name}</td>
                <td>{dns.content}</td>
                <td>{dns.type}</td>
                <td>{dns.ttl}</td>
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
        <CCardHeader>All DNS For {this.state.zoneInfo.zoneName}</CCardHeader>
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
}

const Dns = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()

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

  return <DnsBase location={location} {...props} />
}
export default Dns
