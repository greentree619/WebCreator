import PropTypes from 'prop-types'
import React, { useEffect, useState, createRef } from 'react'
import classNames from 'classnames'
import {
  CRow,
  CCol,
  CCard,
  CButton,
  CCardHeader,
  CCardBody,
  CForm,
  CFormInput,
  CFormLabel,
  CAlert,
  CFormTextarea,
  CDropdown,
  CDropdownItem,
  CDropdownToggle,
  CDropdownMenu,
  CFormSelect,
  CFormSwitch,
  CFormRange,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { CKEditor } from '@ckeditor/ckeditor5-react'
import ClassicEditor from '@ckeditor/ckeditor5-build-classic'

const Setting = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [model, setModel] = useState('text-davinci-003')
  const [prompt, setPrompt] = useState('Write a Blogpost for 500 words about:{{Q}}')
  const [maxTokens, setMaxTokens] = useState(500)
  const [temperature, setTemperature] = useState(1)
  const [topP, setTopP] = useState(1)
  const [n, setN] = useState(500)
  const [presencePenalty, setPresencePenalty] = useState(0)
  const [frequencyPenalty, setFrequencyPenalty] = useState(0)
  const navigate = useNavigate()

  const getAFSettingInfo = async () => {
    const requestOptions = {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}setting/openAISetting`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret) {
      //console.log(ret, ret.data, Number(ret.data.video.toFixed(2)))
      setModel(ret.data.model)
      setPrompt(ret.data.prompt)
      setMaxTokens(ret.data.maxTokens)
      setTemperature(Number(ret.data.temperature.toFixed(1)))
      setTopP(Number(ret.data.topP.toFixed(1)))
      setN(ret.data.n)
      setPresencePenalty(Number(ret.data.presencePenalty.toFixed(1)))
      setFrequencyPenalty(Number(ret.data.frequencyPenalty.toFixed(1)))
    }    
  }

  useEffect(() => {
    getAFSettingInfo()
  }, [])

  const handleSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (form.checkValidity() === false) {
      event.stopPropagation()
    } else {
      updateAFSetting()
    }
    //setValidated(true)
  }

  async function updateAFSetting() {    
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        model: model,
        prompt: prompt,
        maxTokens: Number(maxTokens),
        temperature: Number(temperature),
        topP: Number(topP),
        n: Number(n),
        presencePenalty: Number(presencePenalty),
        frequencyPenalty: Number(frequencyPenalty),
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}setting/openAISetting`, requestOptions)
    setAlertColor('danger')
    setAlertMsg('Faild to update OpenAI API Setting unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      setAlertMsg('Updated OpenAI API Setting successfully.')
      setAlertColor('success')
    }
    setAlarmVisible(true)
  }

  const inputChangeHandler = (setFunction, event) => {
    setFunction(event.target.value)
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>OpenAI API Setting</CCardHeader>
        <CCardBody>
          <CAlert
            color={alertColor}
            dismissible
            visible={alarmVisible}
            onClose={() => setAlarmVisible(false)}
          >
            {alertMsg}
          </CAlert>
          <CForm 
            className="row g-3 needs-validation"
            noValidate
            onSubmit={handleSubmit}
          >
            <div className="mb-3">
              <CRow>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="promptFormControlInput">Prompt</CFormLabel>
                  <CFormInput
                    type="text"
                    id="promptFormControlInput"
                    placeholder=""
                    aria-label="Prompt"
                    required
                    onChange={(e) => inputChangeHandler(setPrompt, e)}
                    value={prompt}
                  />
                  (The prompt(s) to generate completions for, encoded as a string, array of strings, array of tokens, or array of token arrays.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="model">Model</CFormLabel>
                  <CFormSelect id="model" value={model} onChange={(obj) => setModel(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                      <option value="text-davinci-003">text-davinci-003</option>
                      <option value="text-curie-001">text-curie-001</option>
                      <option value="text-babbage-001">text-babbage-001</option>
                      <option value="text-ada-001">text-ada-001</option>
                  </CFormSelect>
                  (ID of the model to use. You can use the List models API to see all of your available models, or see our Model overview for descriptions of them.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={0.0} max={1.0} step={0.1} value={temperature} defaultValue={temperature} onChange={(obj)=>{setTemperature(obj.target.value)}} id="temperature" label={"Temperature (" + temperature + ")"} />
                  <br/>
                  (What sampling temperature to use. Higher values means the model will take more risks. Try 0.9 for more creative applications, and 0 (argmax sampling) for ones with a well-defined answer.
We generally recommend altering this or top_p but not both)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={0.0} max={1.0} step={0.1} value={topP} defaultValue={topP} onChange={(obj)=>{setTopP(obj.target.value)}} id="topP" label={"TopP (" + topP + ")"} />
                  <br/>
                  (An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.
We generally recommend altering this or temperature but not both.)
                </CCol>
              </CRow>
            </div>
            <div className='mb-3'>
              <CRow>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="nVariation">N</CFormLabel>
                  <input type="number" className="form-control" defaultValue={1} onChange={(e) => inputChangeHandler(setN, e)} value={n} min={1} max={100} required></input>
                  (How many completions to generate for each prompt.

Note: Because this parameter generates many completions, it can quickly consume your token quota. Use carefully and ensure that you have reasonable settings for max_tokens and stop.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="maxTokensVariation">Max Tokens</CFormLabel>
                  <input type="number" className="form-control" defaultValue={500} onChange={(e) => inputChangeHandler(setMaxTokens, e)} value={maxTokens} min={1} max={2000} required></input>
                  (The maximum number of tokens to generate in the completion.
The token count of your prompt plus max_tokens cannot exceed the model&apos;s context length. Most models have a context length of 2048 tokens (except for the newest models, which support 4096))
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={-2.0} max={2.0} step={0.1} value={presencePenalty} defaultValue={presencePenalty} onChange={(obj)=>{setPresencePenalty(obj.target.value)}} id="presencePenalty" label={"Presence Penalty (" + presencePenalty + ")"} />
                  <br/>
                  (Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model&apos;s likelihood to talk about new topics.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={-2.0} max={2.0} step={0.1} value={frequencyPenalty} defaultValue={frequencyPenalty} onChange={(obj)=>{setFrequencyPenalty(obj.target.value)}} id="frequencyPenalty" label={"frequency Penalty (" + frequencyPenalty + ")"} />
                  <br/>
                  (Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model&apos;s likelihood to repeat the same line verbatim.)
                </CCol>
              </CRow>
            </div>
            <div>
              <CRow>
                <CCol xs="auto" className="me-auto">
                </CCol>
                <CCol xs="auto">
                  <CButton type="submit">Save</CButton>
                </CCol>
              </CRow>
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default Setting
