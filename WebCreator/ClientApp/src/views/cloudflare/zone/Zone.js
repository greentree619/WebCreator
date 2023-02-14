import React, { Component } from 'react'
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

export default class Zone extends Component {
  static displayName = Zone.name

  constructor(props) {
    super(props)
    this.state = {
      listData: [],
      loading: true,
      alarmVisible: false,
      alertMsg: '',
      alertColor: 'success',
      curPage: 1,
      totalPage: 1,
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
              <th>Zone</th>
              <th>status</th>
              <th>name servers</th>
            </tr>
          </thead>
          <tbody>
            {state.listData.map((row) => (
              <tr key={row.id}>
                <td>{row.id}</td>
                <td>
                  <Link to={`/cloudflare/dns`} state={{ zoneId: row.id, zoneName: row.name }}>
                    {row.name}
                  </Link>
                </td>
                <td>{row.status}</td>
                <td>
                  {row.name_servers.map((serv, index) => {
                    return serv + ', '
                  })}
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
        <em>Loading...</em>
      </p>
    ) : (
      this.renderProjectsTable(this.state)
    )
    return (
      <CCard className="mb-4">
        <CCardHeader>All Zones</CCardHeader>
        <CCardBody>{contents}</CCardBody>
      </CCard>
    )
  }

  async populateData(pageNo) {
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}dns/` + pageNo + '/200')
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
