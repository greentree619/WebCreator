import PropTypes from 'prop-types'
import React, { useEffect, useState, createRef, useRef } from 'react'
import classNames from 'classnames'
import {
  CRow,
  CCol,
  CCard,
  CButton,
  CCardHeader,
  CCardBody,
  CPagination,
  CPaginationItem,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { Outlet, Link } from 'react-router-dom'

const View = (props) => {
  let contents
  const location = useLocation()
  const navigate = useNavigate()
  const [loading, setloading] = useState(false)
  const [logs, setLogs] = useState([])
  const [curPage, setCurPage] = useState(1)
  const [totalPage, setTotalPage] = useState(1)

  if (location.state == null && location.search.length > 0) {
    location.state = {
      category: new URLSearchParams(location.search).get('category'),
      projectId: new URLSearchParams(location.search).get('projectId'),
    }
  }

  useEffect(() => {
    populateHistoryData(1)
  }, [])

  const renderLogsTable = (logs) => {
    let pageButtonCount = 3
    let pagination = <p></p>

    if (totalPage > 1) {
      let prevButton = (
        <CPaginationItem onClick={() => this.gotoPrevPage()}>Previous</CPaginationItem>
      )
      if (curPage <= 1) prevButton = <CPaginationItem disabled>Previous</CPaginationItem>

      let nextButton = <CPaginationItem onClick={() => this.gotoNextPage()}>Next</CPaginationItem>
      if (curPage >= totalPage)
        nextButton = <CPaginationItem disabled>Next</CPaginationItem>

      var pageNoAry = []
      var startNo = curPage - pageButtonCount
      var endNo = curPage + pageButtonCount
      if (startNo < 1) {
        startNo = 1
        endNo =
          pageButtonCount * 2 + 1 > totalPage
            ? totalPage
            : pageButtonCount * 2 + 1
      } else if (endNo > totalPage) {
        endNo = totalPage
        startNo = endNo - pageButtonCount * 2 > 1 ? endNo - pageButtonCount * 2 : 1
      }

      for (var i = startNo; i <= endNo; i++) {
        if (i < 1 || i > totalPage) continue
        pageNoAry.push(i)
      }

      const paginationItems = pageNoAry.map((number) => (
        <CPaginationItem
          key={number}
          onClick={() => populateHistoryData(number)}
          active={number == curPage}
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
        <table className="table">
          <thead>
            <tr>
              <th>Id</th>
              <th>Action</th>
              <th>Date Time</th>
            </tr>
          </thead>
          <tbody>
            {logs.map((log) => {
              //if (log.content != null && log.content.length > 0)
              {
                return (<tr key={log.id}>
                  <td>{log.id}</td>
                  <td>{log.log}</td>
                  <td>{log.createdTime.replace('T', ' ')}</td>
                </tr>)
              }
            })}
          </tbody>
        </table>
        {pagination}
      </>
    )
  }

  const populateHistoryData = async (pageNo) => {
    setloading(true)
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}log/${location.state.projectId}/${location.state.category}/${pageNo}/25`
    )
    const data = await response.json()
    if (response.status === 200 && data)
    {
      setLogs(data.data)
      setCurPage(data.curPage)
      setTotalPage(data.total)
    }
    setloading(false)
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>{location.state.category} History View</CCardHeader>
        <CCardBody>
          {loading ? (
            <p>
              <em>Loading...</em>
            </p>
          ) : (
            renderLogsTable(logs)
          )}
          <div className="mb-3">
            <CButton type="button" onClick={() => navigate(-1)}>
              Back
            </CButton>
          </div>
        </CCardBody>
      </CCard>
    </>
  )
}

export default View